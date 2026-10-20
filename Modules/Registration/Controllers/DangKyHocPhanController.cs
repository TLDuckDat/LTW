using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Services;

namespace StudentManagement.Modules.Registration.Controllers;

/// <summary>
/// Sinh viên chỉ thao tác trên hồ sơ của mình (tài khoản đã liên kết SinhVienId);
/// Admin và PhongDaoTao thao tác thay mặt bất kỳ sinh viên nào.
/// </summary>
[Route("api/dang-ky-hoc-phan")]
[Tags("Registration")]
[Authorize(Roles = "Admin,PhongDaoTao,SinhVien")]
public class DangKyHocPhanController(IDangKyHocPhanService service, ICurrentUserService currentUser) : ApiControllerBase
{
    private bool LaNhanVien => User.IsInRole("Admin") || User.IsInRole("PhongDaoTao");

    // REG-02 Lớp học phần đang mở đăng ký, kèm điều kiện đăng ký của sinh viên
    [HttpGet("lop-mo")]
    public Task<ActionResult<ApiResponse<PagedResult<LopMoDto>>>> GetLopMo([FromQuery] LopMoQuery query) =>
        Execute(async () => await service.GetLopMoAsync(await XacDinhSinhVienAsync(query.SinhVienId), query));

    // REG-03 Đăng ký học phần (kiểm tra đợt, sĩ số, tiên quyết, trùng lịch, tín chỉ)
    [HttpPost]
    public Task<ActionResult<ApiResponse<DangKyDto>>> DangKy([FromBody] DangKyRequest request) =>
        ExecuteCreated(async () => await service.DangKyAsync(
                await XacDinhSinhVienAsync(request.SinhVienId), request.LopHocPhanId, request.XacNhanHocCaiThien),
            x => x.Id, "Đăng ký học phần thành công.");

    // REG-05 Danh sách học phần đã đăng ký trong học kỳ
    [HttpGet("da-dang-ky")]
    public Task<ActionResult<ApiResponse<DaDangKyDto>>> GetDaDangKy([FromQuery] HocKyQuery query) =>
        Execute(async () => await service.GetDaDangKyAsync(await XacDinhSinhVienAsync(query.SinhVienId), query.HocKyId));

    // REG-05 Thời khóa biểu của sinh viên trong học kỳ
    [HttpGet("thoi-khoa-bieu")]
    public Task<ActionResult<ApiResponse<ThoiKhoaBieuDto>>> GetThoiKhoaBieu([FromQuery] HocKyQuery query) =>
        Execute(async () => await service.GetThoiKhoaBieuAsync(await XacDinhSinhVienAsync(query.SinhVienId), query.HocKyId));

    // REG-07 Theo dõi sĩ số các lớp học phần trong học kỳ (lọc lớp đầy/gần đầy)
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpGet("si-so")]
    public Task<ActionResult<ApiResponse<PagedResult<SiSoLopDto>>>> GetSiSo([FromQuery] SiSoQuery query) =>
        Execute(() => service.GetSiSoAsync(query));

    // REG-07 Cập nhật sĩ số tối đa của lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("si-so/{lopHocPhanId:int}")]
    public Task<ActionResult<ApiResponse<SiSoLopDto>>> CapNhatSiSo(int lopHocPhanId, [FromBody] CapNhatSiSoRequest request) =>
        Execute(() => service.CapNhatSiSoToiDaAsync(lopHocPhanId, request.SiSoToiDa), message: "Đã cập nhật sĩ số tối đa.");

    // REG-04 Hủy đăng ký học phần (id là mã lượt đăng ký); sinh viên chỉ hủy được lượt của mình
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<DangKyDto>>> HuyDangKy(int id) =>
        Execute(async () => await service.HuyDangKyAsync(id, LaNhanVien ? null : await XacDinhSinhVienAsync(null)),
            message: "Đã hủy đăng ký học phần.");

    /// <summary>
    /// Sinh viên được thao tác: nhân viên phải truyền sinhVienId (400 nếu thiếu); sinh viên chỉ được là chính mình
    /// (403 nếu chưa liên kết hồ sơ hoặc truyền sinhVienId của người khác).
    /// </summary>
    private async Task<int> XacDinhSinhVienAsync(int? sinhVienIdYeuCau)
    {
        if (LaNhanVien)
            return sinhVienIdYeuCau ?? throw AppException.BadRequest("Cần truyền sinhVienId khi thao tác thay sinh viên.");

        var sinhVienCuaToi = await currentUser.GetSinhVienIdAsync();
        if (sinhVienIdYeuCau is not null && sinhVienIdYeuCau != sinhVienCuaToi)
            throw AppException.Forbidden("Không được thao tác trên hồ sơ của sinh viên khác.");
        return sinhVienCuaToi;
    }
}
