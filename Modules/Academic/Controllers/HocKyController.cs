using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/hoc-ky")]
[Tags("Academic")]
public class HocKyController(IHocKyService service) : ControllerBase
{
    private readonly IHocKyService _service = service;

    // EDU-03 Danh sách học kỳ (lọc theo năm học)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<HocKyDto>>>> GetPaged([FromQuery] HocKyFilterQuery query)
        => Ok(ApiResponse<PagedResult<HocKyDto>>.Ok(await _service.GetPagedAsync(query)));

    // EDU-03 Chi tiết học kỳ
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<HocKyDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<HocKyDto>.Fail("Không tìm thấy học kỳ."))
            : Ok(ApiResponse<HocKyDto>.Ok(result));
    }

    // EDU-03 Thêm học kỳ
    [HttpPost]
    public async Task<ActionResult<ApiResponse<HocKyDto>>> Create([FromBody] HocKyRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<HocKyDto>.Ok(result, "Đã tạo học kỳ."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<HocKyDto>.Fail(ex.Message));
        }
    }

    // EDU-03 Cập nhật học kỳ
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<HocKyDto>>> Update(int id, [FromBody] HocKyRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<HocKyDto>.Fail("Không tìm thấy học kỳ."))
                : Ok(ApiResponse<HocKyDto>.Ok(result, "Đã cập nhật học kỳ."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<HocKyDto>.Fail(ex.Message));
        }
    }

    // EDU-03 Xóa học kỳ
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa học kỳ."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy học kỳ."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
