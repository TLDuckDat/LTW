using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[Route("api/bo-mon")]
[Tags("Academic")]
public class BoMonController(IBoMonService service) : ApiControllerBase
{
    // EDU-01 Danh sách bộ môn (phân trang, lọc theo khoa, tìm kiếm)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<BoMonDto>>>> GetPaged([FromQuery] BoMonFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // EDU-01 Chi tiết bộ môn
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<BoMonDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // EDU-01 Thêm bộ môn
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<BoMonDto>>> Create([FromBody] BoMonRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo bộ môn.");

    // EDU-01 Cập nhật bộ môn
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<BoMonDto>>> Update(int id, [FromBody] BoMonRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật bộ môn.");

    // EDU-01 Xóa bộ môn
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa bộ môn.");
}
