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

    /// <summary>
    /// Xác định sinh viên được thao tác. Trả về lỗi 400/403 khi thiếu hoặc không được phép.
    /// </summary>
    private async Task<(int? SinhVienId, ActionResult? Loi)> XacDinhSinhVienAsync(int? sinhVienIdYeuCau)
    {
        if (LaNhanVien)
        {
            return sinhVienIdYeuCau is null
                ? (null, BadRequest(ApiResponse<object>.Fail("Cần truyền sinhVienId khi đăng ký thay sinh viên.")))
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
