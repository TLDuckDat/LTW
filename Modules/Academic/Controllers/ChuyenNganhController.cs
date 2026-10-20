using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[Route("api/chuyen-nganh")]
[Tags("Academic")]
public class ChuyenNganhController(IChuyenNganhService service) : ApiControllerBase
{
    // EDU-02 Danh sách chuyên ngành (phân trang, lọc theo ngành, tìm kiếm)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<ChuyenNganhDto>>>> GetPaged([FromQuery] ChuyenNganhFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // EDU-02 Chi tiết chuyên ngành
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<ChuyenNganhDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // EDU-02 Thêm chuyên ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<ChuyenNganhDto>>> Create([FromBody] ChuyenNganhRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo chuyên ngành.");

    // EDU-02 Cập nhật chuyên ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<ChuyenNganhDto>>> Update(int id, [FromBody] ChuyenNganhRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật chuyên ngành.");

    // EDU-02 Xóa chuyên ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa chuyên ngành.");
}
