using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

/// <summary>GRADE-01: danh sách sinh viên trong lớp học phần.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/diem/lop-hoc-phan/{lopHocPhanId:int}/sinh-vien")]
[Tags("Grades - Sinh viên trong lớp")]
public class SinhVienLopHocPhanController(ISinhVienLopHocPhanService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<SinhVienTrongLopDto>>>> GetDanhSach(
        int lopHocPhanId, [FromQuery] PagedQuery query) =>
        Execute(() => service.GetDanhSachAsync(lopHocPhanId, query));

    [HttpPost]
    public Task<ActionResult<ApiResponse<DangKyHocPhanDto>>> ThemSinhVien(
        int lopHocPhanId, [FromBody] ThemSinhVienRequest request) =>
        Execute(() => service.ThemSinhVienAsync(lopHocPhanId, request),
            StatusCodes.Status201Created, "Đã thêm sinh viên vào lớp.");

    [HttpPut("{dangKyHocPhanId:int}")]
    public Task<ActionResult<ApiResponse<DangKyHocPhanDto>>> ChuyenLop(
        int lopHocPhanId, int dangKyHocPhanId, [FromBody] ChuyenLopRequest request) =>
        Execute(() => service.ChuyenLopAsync(lopHocPhanId, dangKyHocPhanId, request),
            message: "Đã chuyển sinh viên sang lớp mới.");

    [HttpDelete("{dangKyHocPhanId:int}")]
    public Task<ActionResult<ApiResponse<object>>> HuyDangKy(int lopHocPhanId, int dangKyHocPhanId) =>
        Execute(() => service.HuyDangKyAsync(lopHocPhanId, dangKyHocPhanId),
            "Đã hủy đăng ký của sinh viên trong lớp.");
}
