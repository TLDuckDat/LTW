using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[Route("api/quy-che")]
[Tags("Academic")]
public class QuyCheHocVuController(IQuyCheHocVuService service) : ApiControllerBase
{
    // EDU-06 Danh sách quy chế và điều kiện học vụ
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<QuyCheHocVuDto>>>> GetPaged([FromQuery] QuyCheHocVuFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // EDU-06 Chi tiết quy chế theo Id
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // EDU-06 Tra cứu quy chế theo Mã (để các module khác như Điểm, Đăng ký dễ gọi)
    [HttpGet("ma/{ma}")]
    public Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> GetByMa(string ma) =>
        Execute(() => service.GetByMaAsync(ma));

    // EDU-06 Thêm quy chế
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> Create([FromBody] QuyCheHocVuRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã tạo quy chế học vụ.");

    // EDU-06 Cập nhật quy chế
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> Update(int id, [FromBody] QuyCheHocVuRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật quy chế học vụ.");

    // EDU-06 Xóa quy chế
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa quy chế học vụ.");
}
