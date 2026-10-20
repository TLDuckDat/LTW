using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[Route("api/nganh")]
[Tags("Academic")]
public class NganhController(INganhService service) : ApiControllerBase
{
    // EDU-02 Danh sách ngành (phân trang, lọc theo khoa, tìm kiếm)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<NganhDto>>>> GetPaged([FromQuery] NganhFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // EDU-02 Chi tiết ngành
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<NganhDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // EDU-02 Thêm ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<NganhDto>>> Create([FromBody] NganhRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo ngành đào tạo.");

    // EDU-02 Cập nhật ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<NganhDto>>> Update(int id, [FromBody] NganhRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật ngành đào tạo.");

    // EDU-02 Xóa ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa ngành đào tạo.");
}
