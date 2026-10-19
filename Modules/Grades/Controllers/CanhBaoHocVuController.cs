using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

/// <summary>GRADE-07: cảnh báo học vụ theo học kỳ.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/diem")]
[Tags("Grades - Cảnh báo học vụ")]
public class CanhBaoHocVuController(ICanhBaoHocVuService service) : ApiControllerBase
{
    // Đánh giá của một sinh viên (kể cả khi chưa đủ dữ liệu hoặc không bị cảnh báo)
    [HttpGet("sinh-vien/{sinhVienId:int}/canh-bao")]
    public Task<ActionResult<ApiResponse<CanhBaoHocVuDto>>> GetCanhBao(
        int sinhVienId, [FromQuery, Range(1, int.MaxValue)] int hocKyId) =>
        Execute(() => service.GetCanhBaoAsync(sinhVienId, hocKyId));

    // Danh sách sinh viên bị cảnh báo trong học kỳ
    [HttpGet("canh-bao")]
    public Task<ActionResult<ApiResponse<PagedResult<CanhBaoHocVuDto>>>> GetDanhSachCanhBao(
        [FromQuery, Range(1, int.MaxValue)] int hocKyId, [FromQuery] PagedQuery query) =>
        Execute(() => service.GetDanhSachCanhBaoAsync(hocKyId, query));
}
