using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Services;

namespace StudentManagement.Modules.Registration.Controllers;

[ApiController]
[Route("api/dot-dang-ky")]
[Tags("Registration")]
public class DotDangKyController(IDotDangKyService service) : ControllerBase
{
    private readonly IDotDangKyService _service = service;

    // REG-01 Danh sách đợt đăng ký (lọc theo học kỳ, trạng thái)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DotDangKyDto>>>> GetPaged([FromQuery] DotDangKyQuery query)
        => Ok(ApiResponse<PagedResult<DotDangKyDto>>.Ok(await _service.GetPagedAsync(query)));

    // REG-01 Chi tiết đợt đăng ký
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<DotDangKyDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<DotDangKyDto>.Fail("Không tìm thấy đợt đăng ký."))
            : Ok(ApiResponse<DotDangKyDto>.Ok(result));
    }

    // REG-01 Tạo đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DotDangKyDto>>> Create([FromBody] DotDangKyRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DotDangKyDto>.Ok(result, "Đã tạo đợt đăng ký."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<DotDangKyDto>.Fail(ex.Message));
        }
    }

    // REG-01 Cập nhật tên, thời gian đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<DotDangKyDto>>> Update(int id, [FromBody] DotDangKyUpdateRequest request)
        => await XuLyAsync(() => _service.UpdateAsync(id, request), "Đã cập nhật đợt đăng ký.");

    // REG-01 Mở đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/mo")]
    public async Task<ActionResult<ApiResponse<DotDangKyDto>>> Mo(int id)
        => await XuLyAsync(() => _service.MoAsync(id), "Đã mở đợt đăng ký.");

    // REG-01 Đóng đợt đăng ký
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/dong")]
    public async Task<ActionResult<ApiResponse<DotDangKyDto>>> Dong(int id)
        => await XuLyAsync(() => _service.DongAsync(id), "Đã đóng đợt đăng ký.");

    // REG-01 Xóa đợt đăng ký chưa mở
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa đợt đăng ký."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy đợt đăng ký."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    private async Task<ActionResult<ApiResponse<DotDangKyDto>>> XuLyAsync(Func<Task<DotDangKyDto?>> action, string thongBao)
    {
        try
        {
            var result = await action();
            return result is null
                ? NotFound(ApiResponse<DotDangKyDto>.Fail("Không tìm thấy đợt đăng ký."))
                : Ok(ApiResponse<DotDangKyDto>.Ok(result, thongBao));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<DotDangKyDto>.Fail(ex.Message));
        }
    }
}
