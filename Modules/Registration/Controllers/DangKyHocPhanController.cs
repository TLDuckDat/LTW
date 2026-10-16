using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Services;

namespace StudentManagement.Modules.Registration.Controllers;

/// <summary>
/// Sinh viên chỉ thao tác trên hồ sơ của mình (tài khoản đã liên kết SinhVienId);
/// Admin và PhongDaoTao thao tác thay mặt bất kỳ sinh viên nào.
/// </summary>
[ApiController]
[Route("api/dang-ky-hoc-phan")]
[Tags("Registration")]
[Authorize(Roles = "Admin,PhongDaoTao,SinhVien")]
public class DangKyHocPhanController(IDangKyHocPhanService service) : ControllerBase
{
    private readonly IDangKyHocPhanService _service = service;

    private bool LaNhanVien => User.IsInRole("Admin") || User.IsInRole("PhongDaoTao");

    // REG-02 Lớp học phần đang mở đăng ký, kèm điều kiện đăng ký của sinh viên
    [HttpGet("lop-mo")]
    public async Task<ActionResult<ApiResponse<PagedResult<LopMoDto>>>> GetLopMo([FromQuery] LopMoQuery query)
    {
        var (sinhVienId, loi) = await XacDinhSinhVienAsync(query.SinhVienId);
        if (loi is not null) return loi;

        var result = await _service.GetLopMoAsync(sinhVienId!.Value, query);
        return result is null
            ? NotFound(ApiResponse<PagedResult<LopMoDto>>.Fail("Không tìm thấy sinh viên hoặc học kỳ."))
            : Ok(ApiResponse<PagedResult<LopMoDto>>.Ok(result));
    }

    // REG-03 Đăng ký học phần (kiểm tra đợt, sĩ số, tiên quyết, trùng lịch, tín chỉ)
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DangKyDto>>> DangKy([FromBody] DangKyRequest request)
    {
        var (sinhVienId, loi) = await XacDinhSinhVienAsync(request.SinhVienId);
        if (loi is not null) return loi;

        try
        {
            var result = await _service.DangKyAsync(sinhVienId!.Value, request.LopHocPhanId);
            return result is null
                ? NotFound(ApiResponse<DangKyDto>.Fail("Không tìm thấy sinh viên hoặc lớp học phần."))
                : StatusCode(StatusCodes.Status201Created, ApiResponse<DangKyDto>.Ok(result, "Đăng ký học phần thành công."));
        }
        catch (DangKyKhongHopLeException ex)
        {
            return Conflict(new ApiResponse<DangKyDto> { Success = false, Message = ex.Message });
        }
    }

    // REG-04 Hủy đăng ký học phần (id là mã lượt đăng ký)
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<DangKyDto>>> HuyDangKy(int id)
    {
        int? chiSinhVienId = null;
        if (!LaNhanVien)
        {
            var (sinhVienId, loi) = await XacDinhSinhVienAsync(null);
            if (loi is not null) return loi;
            chiSinhVienId = sinhVienId;
        }

        try
        {
            var result = await _service.HuyDangKyAsync(id, chiSinhVienId);
            return result is null
                ? NotFound(ApiResponse<DangKyDto>.Fail("Không tìm thấy lượt đăng ký."))
                : Ok(ApiResponse<DangKyDto>.Ok(result, "Đã hủy đăng ký học phần."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<DangKyDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Xác định sinh viên được thao tác. Trả về lỗi 400/403 khi thiếu hoặc không được phép.
    /// </summary>
    private async Task<(int? SinhVienId, ActionResult? Loi)> XacDinhSinhVienAsync(int? sinhVienIdYeuCau)
    {
        if (LaNhanVien)
        {
            return sinhVienIdYeuCau is null
                ? (null, BadRequest(ApiResponse<object>.Fail("Cần truyền sinhVienId khi thao tác thay sinh viên.")))
                : (sinhVienIdYeuCau, null);
        }

        var sinhVienCuaToi = int.TryParse(User.FindFirst("sub")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var taiKhoanId)
            ? await _service.GetSinhVienIdCuaTaiKhoanAsync(taiKhoanId)
            : null;
        if (sinhVienCuaToi is null)
            return (null, Forbidden("Tài khoản chưa được liên kết với hồ sơ sinh viên."));
        if (sinhVienIdYeuCau is not null && sinhVienIdYeuCau != sinhVienCuaToi)
            return (null, Forbidden("Không được thao tác trên hồ sơ của sinh viên khác."));
        return (sinhVienCuaToi, null);
    }

    private ObjectResult Forbidden(string message)
        => StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(message));
}
