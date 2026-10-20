using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao,GiangVien")]
[Route("api/lop-hanh-chinh")]
[Tags("Students - LopHanhChinh")]
public class LopHanhChinhController(ILopHanhChinhService service) : ApiControllerBase
{
    // STU-04: Danh sách lớp hành chính
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<LopHanhChinhDto>>>> GetPaged(
        [FromQuery] PagedQuery query,
        [FromQuery] int? nganhId,
        [FromQuery] int? khoaTuyen) =>
        Execute(() => service.GetPagedAsync(query, nganhId, khoaTuyen));

    // STU-04: Chi tiết lớp hành chính kèm danh sách sinh viên
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<LopHanhChinhDetailDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // STU-04: Tạo lớp hành chính
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<LopHanhChinhDto>>> Create([FromBody] LopHanhChinhRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo lớp hành chính.");

    // STU-04: Sửa lớp hành chính
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<LopHanhChinhDto>>> Update(int id, [FromBody] LopHanhChinhRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật lớp hành chính.");

    // STU-04: Xóa lớp hành chính
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa lớp hành chính.");

    // STU-04: Phân nhiều sinh viên vào lớp
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/phan-lop")]
    public Task<ActionResult<ApiResponse<object>>> PhanLop(int id, [FromBody] PhanLopRequest request) =>
        Execute(() => service.PhanLopAsync(id, request), "Đã phân bổ sinh viên vào lớp.");

    // STU-05: Gán cố vấn học tập cho lớp
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}/co-van")]
    public Task<ActionResult<ApiResponse<object>>> PhanCoVan(int id, [FromBody] PhanCoVanRequest request) =>
        Execute(() => service.PhanCoVanAsync(id, request), "Đã cập nhật cố vấn học tập.");
}
