using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/nam-hoc")]
[Tags("Academic")]
public class NamHocController(INamHocService service) : ControllerBase
{
    private readonly INamHocService _service = service;

    // EDU-03 Danh sách năm học
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NamHocDto>>>> GetPaged([FromQuery] PagedQuery query)
        => Ok(ApiResponse<PagedResult<NamHocDto>>.Ok(await _service.GetPagedAsync(query)));

    // EDU-03 Chi tiết năm học
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<NamHocDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<NamHocDto>.Fail("Không tìm thấy năm học."))
            : Ok(ApiResponse<NamHocDto>.Ok(result));
    }

    // EDU-03 Thêm năm học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<NamHocDto>>> Create([FromBody] NamHocRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<NamHocDto>.Ok(result, "Đã tạo năm học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<NamHocDto>.Fail(ex.Message));
        }
    }

    // EDU-03 Cập nhật năm học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<NamHocDto>>> Update(int id, [FromBody] NamHocRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<NamHocDto>.Fail("Không tìm thấy năm học."))
                : Ok(ApiResponse<NamHocDto>.Ok(result, "Đã cập nhật năm học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<NamHocDto>.Fail(ex.Message));
        }
    }

    // EDU-03 Xóa năm học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa năm học."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy năm học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
