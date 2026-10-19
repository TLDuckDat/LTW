using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/quy-che")]
[Tags("Academic")]
public class QuyCheHocVuController(IQuyCheHocVuService service) : ControllerBase
{
    private readonly IQuyCheHocVuService _service = service;

    // EDU-06 Danh sách quy chế và điều kiện học vụ
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<QuyCheHocVuDto>>>> GetPaged([FromQuery] QuyCheHocVuFilterQuery query)
        => Ok(ApiResponse<PagedResult<QuyCheHocVuDto>>.Ok(await _service.GetPagedAsync(query)));

    // EDU-06 Chi tiết quy chế theo Id
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<QuyCheHocVuDto>.Fail("Không tìm thấy quy chế học vụ."))
            : Ok(ApiResponse<QuyCheHocVuDto>.Ok(result));
    }

    // EDU-06 Tra cứu quy chế theo Mã (để các module khác như Điểm, Đăng ký dễ gọi)
    [HttpGet("ma/{ma}")]
    public async Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> GetByMa(string ma)
    {
        var result = await _service.GetByMaAsync(ma);
        return result is null
            ? NotFound(ApiResponse<QuyCheHocVuDto>.Fail($"Không tìm thấy quy chế với mã '{ma}'."))
            : Ok(ApiResponse<QuyCheHocVuDto>.Ok(result));
    }

    // EDU-06 Thêm quy chế
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> Create([FromBody] QuyCheHocVuRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<QuyCheHocVuDto>.Ok(result, "Đã tạo quy chế học vụ."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<QuyCheHocVuDto>.Fail(ex.Message));
        }
    }

    // EDU-06 Cập nhật quy chế
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<QuyCheHocVuDto>>> Update(int id, [FromBody] QuyCheHocVuRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<QuyCheHocVuDto>.Fail("Không tìm thấy quy chế học vụ."))
                : Ok(ApiResponse<QuyCheHocVuDto>.Ok(result, "Đã cập nhật quy chế học vụ."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<QuyCheHocVuDto>.Fail(ex.Message));
        }
    }

    // EDU-06 Xóa quy chế
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        return await _service.DeleteAsync(id)
            ? Ok(ApiResponse<object>.Ok(null, "Đã xóa quy chế học vụ."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy quy chế học vụ."));
    }
}
