using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

[Route("api/giang-vien")]
[Tags("Students - GiangVien")]
public class GiangVienController(IGiangVienService service) : ApiControllerBase
{
    // STU-05: Danh sách giảng viên (phân trang, lọc theo bộ môn, tìm kiếm)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<GiangVienDto>>>> GetPaged([FromQuery] GiangVienFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // STU-05: Chi tiết giảng viên
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<GiangVienDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // STU-05: Thêm giảng viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<GiangVienDto>>> Create([FromBody] GiangVienRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã thêm giảng viên.");

    // STU-05: Cập nhật giảng viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<GiangVienDto>>> Update(int id, [FromBody] GiangVienRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật giảng viên.");

    // STU-05: Xóa giảng viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa giảng viên.");
}
