using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

/// <summary>
/// Điểm theo lớp học phần, theo thứ tự: nhập điểm → tính điểm → duyệt và khóa.
/// Giảng viên chỉ xem, nhập và tính điểm cho lớp mình được phân công; duyệt và khóa do Phòng đào tạo.
/// </summary>
[Route("api/diem/lop-hoc-phan/{lopHocPhanId:int}")]
[Tags("Grades - Nhập và khóa điểm")]
public class DiemController(IDiemService service, ICurrentUserService currentUser) : ApiControllerBase
{
    // Bảng điểm của lớp
    [Authorize(Roles = "Admin,PhongDaoTao,GiangVien")]
    [HttpGet]
    public Task<ActionResult<ApiResponse<BangDiemLopDto>>> GetBangDiemLop(int lopHocPhanId) =>
        Execute(async () => await service.GetBangDiemLopAsync(
            lopHocPhanId, await currentUser.GetGiangVienIdNeuLaGiangVienAsync()));

    // GRADE-02
    [Authorize(Roles = "Admin,PhongDaoTao,GiangVien")]
    [HttpPut]
    public Task<ActionResult<ApiResponse<NhapDiemResultDto>>> NhapDiem(
        int lopHocPhanId, [FromBody] NhapDiemRequest request) =>
        Execute(async () => await service.NhapDiemAsync(
            lopHocPhanId, request, await currentUser.GetGiangVienIdNeuLaGiangVienAsync()), message: "Đã lưu điểm.");

    // GRADE-03
    [Authorize(Roles = "Admin,PhongDaoTao,GiangVien")]
    [HttpPost("tinh-diem")]
    public Task<ActionResult<ApiResponse<TinhDiemResultDto>>> TinhDiem(int lopHocPhanId) =>
        Execute(async () => await service.TinhDiemAsync(
            lopHocPhanId, await currentUser.GetGiangVienIdNeuLaGiangVienAsync()), message: "Đã tính điểm tổng kết.");

    // GRADE-04
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("khoa")]
    public Task<ActionResult<ApiResponse<KhoaDiemResultDto>>> KhoaDiem(int lopHocPhanId) =>
        Execute(() => service.KhoaDiemAsync(lopHocPhanId), message: "Đã duyệt và khóa bảng điểm.");
}
