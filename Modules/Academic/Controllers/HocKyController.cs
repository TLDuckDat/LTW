using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[Route("api/hoc-ky")]
[Tags("Academic")]
public class HocKyController(IHocKyService service) : ApiControllerBase
{
    // EDU-03 Danh sách học kỳ (lọc theo năm học)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<HocKyDto>>>> GetPaged([FromQuery] HocKyFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // EDU-03 Chi tiết học kỳ
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<HocKyDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // EDU-03 Thêm học kỳ
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<HocKyDto>>> Create([FromBody] HocKyRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo học kỳ.");

    // EDU-03 Cập nhật học kỳ
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<HocKyDto>>> Update(int id, [FromBody] HocKyRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật học kỳ.");

    // EDU-03 Xóa học kỳ
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa học kỳ.");
}
