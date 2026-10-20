using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

/// <summary>MẪU CRUD: Controller chỉ gọi Service và bọc kết quả trong ApiResponse.</summary>
[Route("api/khoa")]
[Tags("Academic")]
public class KhoaController(IKhoaService service) : ApiControllerBase
{
    // ACA-01 Danh sách khoa (phân trang, tìm kiếm theo mã/tên)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<KhoaDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // ACA-01 Chi tiết khoa
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<KhoaDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // ACA-01 Thêm khoa
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<KhoaDto>>> Create([FromBody] KhoaRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo khoa.");

    // ACA-01 Cập nhật khoa
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<KhoaDto>>> Update(int id, [FromBody] KhoaRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật khoa.");

    // ACA-01 Xóa khoa
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa khoa.");
}
