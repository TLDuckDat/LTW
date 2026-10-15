using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/nganh")]
[Tags("Academic")]
public class NganhController(INganhService service) : ControllerBase
{
    private readonly INganhService _service = service;

    // EDU-02 Danh sách ngành (phân trang, lọc theo khoa, tìm kiếm)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NganhDto>>>> GetPaged([FromQuery] NganhFilterQuery query)
        => Ok(ApiResponse<PagedResult<NganhDto>>.Ok(await _service.GetPagedAsync(query)));

    // EDU-02 Chi tiết ngành
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<NganhDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<NganhDto>.Fail("Không tìm thấy ngành đào tạo."))
            : Ok(ApiResponse<NganhDto>.Ok(result));
    }

    // EDU-02 Thêm ngành
    [HttpPost]
    public async Task<ActionResult<ApiResponse<NganhDto>>> Create([FromBody] NganhRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<NganhDto>.Ok(result, "Đã tạo ngành đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<NganhDto>.Fail(ex.Message));
        }
    }

    // EDU-02 Cập nhật ngành
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<NganhDto>>> Update(int id, [FromBody] NganhRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<NganhDto>.Fail("Không tìm thấy ngành đào tạo."))
                : Ok(ApiResponse<NganhDto>.Ok(result, "Đã cập nhật ngành đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<NganhDto>.Fail(ex.Message));
        }
    }

    // EDU-02 Xóa ngành
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa ngành đào tạo."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy ngành đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
