using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Services;

namespace StudentManagement.Modules.Registration.Controllers;

[Route("api/dot-dang-ky")]
[Tags("Registration")]
public class DotDangKyController(IDotDangKyService service) : ApiControllerBase
{
    // REG-01 Danh sách đợt đăng ký (lọc theo học kỳ, trạng thái)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<DotDangKyDto>>>> GetPaged([FromQuery] DotDangKyQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // REG-01 Chi tiết đợt đăng ký
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<DotDangKyDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // REG-01 Tạo đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<DotDangKyDto>>> Create([FromBody] DotDangKyRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo đợt đăng ký.");

    // REG-01 Cập nhật tên, thời gian đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<DotDangKyDto>>> Update(int id, [FromBody] DotDangKyUpdateRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật đợt đăng ký.");

    // REG-01 Mở đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/mo")]
    public Task<ActionResult<ApiResponse<DotDangKyDto>>> Mo(int id) =>
        Execute(() => service.MoAsync(id), message: "Đã mở đợt đăng ký.");

    // REG-01 Đóng đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/dong")]
    public Task<ActionResult<ApiResponse<DotDangKyDto>>> Dong(int id) =>
        Execute(() => service.DongAsync(id), message: "Đã đóng đợt đăng ký.");

    // REG-01 Xóa đợt đăng ký chưa mở
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa đợt đăng ký.");
}
