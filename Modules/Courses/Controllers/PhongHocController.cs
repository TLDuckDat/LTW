using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[Route("api/phong-hoc")]
[Tags("Courses")]
public class PhongHocController(IPhongHocService service) : ApiControllerBase
{
    // CRS-04 Danh sách phòng học
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<PhongHocDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // CRS-04 Chi tiết phòng học
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<PhongHocDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // CRS-04 Thêm phòng học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<PhongHocDto>>> Create([FromBody] PhongHocRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo phòng học.");

    // CRS-04 Cập nhật phòng học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<PhongHocDto>>> Update(int id, [FromBody] PhongHocRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật phòng học.");

    // CRS-04 Xóa phòng học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa phòng học.");
}
