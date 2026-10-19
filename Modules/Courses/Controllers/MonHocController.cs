using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[Route("api/mon-hoc")]
[Tags("Courses")]
public class MonHocController(IMonHocService service) : ApiControllerBase
{
    // CRS-01 Danh sách môn học (phân trang, tìm kiếm theo mã/tên)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<MonHocDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // CRS-01 Chi tiết môn học
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<MonHocDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // CRS-01 Thêm môn học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<MonHocDto>>> Create([FromBody] MonHocRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã tạo môn học.");

    // CRS-01 Cập nhật môn học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<MonHocDto>>> Update(int id, [FromBody] MonHocRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật môn học.");

    // CRS-01 Xóa môn học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa môn học.");

    // CRS-01 Danh sách môn tiên quyết của một môn học
    [HttpGet("{id:int}/tien-quyet")]
    public Task<ActionResult<ApiResponse<IReadOnlyList<MonHocDto>>>> GetTienQuyet(int id) =>
        Execute(() => service.GetTienQuyetAsync(id));

    // CRS-01 Thêm môn tiên quyết
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/tien-quyet")]
    public Task<ActionResult<ApiResponse<MonHocDto>>> AddTienQuyet(int id, [FromBody] TienQuyetRequest request) =>
        Execute(() => service.AddTienQuyetAsync(id, request), StatusCodes.Status201Created, "Đã thêm môn tiên quyết.");

    // CRS-01 Gỡ môn tiên quyết
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}/tien-quyet/{tienQuyetId:int}")]
    public Task<ActionResult<ApiResponse<object>>> RemoveTienQuyet(int id, int tienQuyetId) =>
        Execute(() => service.RemoveTienQuyetAsync(id, tienQuyetId), "Đã gỡ môn tiên quyết.");
}
