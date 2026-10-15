using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/bo-mon")]
[Tags("Academic")]
public class BoMonController(IBoMonService service) : ControllerBase
{
    private readonly IBoMonService _service = service;

    // EDU-01 Danh sách bộ môn (phân trang, lọc theo khoa, tìm kiếm)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BoMonDto>>>> GetPaged([FromQuery] BoMonFilterQuery query)
        => Ok(ApiResponse<PagedResult<BoMonDto>>.Ok(await _service.GetPagedAsync(query)));

    // EDU-01 Chi tiết bộ môn
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BoMonDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<BoMonDto>.Fail("Không tìm thấy bộ môn."))
            : Ok(ApiResponse<BoMonDto>.Ok(result));
    }

    // EDU-01 Thêm bộ môn
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BoMonDto>>> Create([FromBody] BoMonRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BoMonDto>.Ok(result, "Đã tạo bộ môn."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<BoMonDto>.Fail(ex.Message));
        }
    }

    // EDU-01 Cập nhật bộ môn
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<BoMonDto>>> Update(int id, [FromBody] BoMonRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<BoMonDto>.Fail("Không tìm thấy bộ môn."))
                : Ok(ApiResponse<BoMonDto>.Ok(result, "Đã cập nhật bộ môn."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<BoMonDto>.Fail(ex.Message));
        }
    }

    // EDU-01 Xóa bộ môn
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa bộ môn."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy bộ môn."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
