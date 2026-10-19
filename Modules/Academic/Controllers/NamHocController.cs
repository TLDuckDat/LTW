using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[Route("api/nam-hoc")]
[Tags("Academic")]
public class NamHocController(INamHocService service) : ApiControllerBase
{
    // EDU-03 Danh sách năm học
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<NamHocDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // EDU-03 Chi tiết năm học
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<NamHocDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // EDU-03 Thêm năm học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<NamHocDto>>> Create([FromBody] NamHocRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã tạo năm học.");

    // EDU-03 Cập nhật năm học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<NamHocDto>>> Update(int id, [FromBody] NamHocRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật năm học.");

    // EDU-03 Xóa năm học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa năm học.");
}
