using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

/// <summary>GRADE-05: phúc khảo điểm cuối kỳ.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/phuc-khao")]
[Tags("Grades - Phúc khảo")]
public class PhucKhaoController(IPhucKhaoService service) : ApiControllerBase
{
    [HttpPost]
    public Task<ActionResult<ApiResponse<PhucKhaoDto>>> Create([FromBody] PhucKhaoRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo yêu cầu phúc khảo.");

    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<PhucKhaoDto>>>> GetPaged(
        [FromQuery] PagedQuery query, [FromQuery] TrangThaiPhucKhao? trangThai) =>
        Execute(() => service.GetPagedAsync(query, trangThai));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<PhucKhaoDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost("{id:int}/xu-ly")]
    public Task<ActionResult<ApiResponse<PhucKhaoDto>>> XuLy(int id, [FromBody] XuLyPhucKhaoRequest request) =>
        Execute(() => service.XuLyAsync(id, request), message: "Đã xử lý yêu cầu phúc khảo.");
}
