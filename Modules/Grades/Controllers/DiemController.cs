using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

/// <summary>Điểm theo lớp học phần, theo thứ tự: nhập điểm → tính điểm → duyệt và khóa.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/diem/lop-hoc-phan/{lopHocPhanId:int}")]
[Tags("Grades - Nhập và khóa điểm")]
public class DiemController(IDiemService service) : ApiControllerBase
{
    // GRADE-02
    [HttpPut]
    public Task<ActionResult<ApiResponse<NhapDiemResultDto>>> NhapDiem(
        int lopHocPhanId, [FromBody] NhapDiemRequest request) =>
        Execute(() => service.NhapDiemAsync(lopHocPhanId, request), message: "Đã lưu điểm.");

    // GRADE-03
    [HttpPost("tinh-diem")]
    public Task<ActionResult<ApiResponse<TinhDiemResultDto>>> TinhDiem(int lopHocPhanId) =>
        Execute(() => service.TinhDiemAsync(lopHocPhanId), message: "Đã tính điểm tổng kết.");

    // GRADE-04
    [HttpPost("khoa")]
    public Task<ActionResult<ApiResponse<KhoaDiemResultDto>>> KhoaDiem(int lopHocPhanId) =>
        Execute(() => service.KhoaDiemAsync(lopHocPhanId), message: "Đã duyệt và khóa bảng điểm.");
}
