using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

/// <summary>GRADE-06: bảng điểm sinh viên, GPA học kỳ và CPA tích lũy.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/diem/sinh-vien/{sinhVienId:int}")]
[Tags("Grades - Bảng điểm")]
public class BangDiemController(IBangDiemService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<BangDiemSinhVienDto>>> GetBangDiem(int sinhVienId) =>
        Execute(() => service.GetBangDiemAsync(sinhVienId));
}
