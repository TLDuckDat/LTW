using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

[ApiController]
[Route("api/giang-vien")]
[Tags("Students - GiangVien")]
public class GiangVienController(IGiangVienService service) : ControllerBase
{
    private readonly IGiangVienService _service = service;

    // STU-05: Danh sách giảng viên (phân trang, lọc theo bộ môn, tìm kiếm)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<GiangVienDto>>>> GetPaged([FromQuery] GiangVienFilterQuery query)
        => Ok(ApiResponse<PagedResult<GiangVienDto>>.Ok(await _service.GetPagedAsync(query)));

    // STU-05: Chi tiết giảng viên
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<GiangVienDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<GiangVienDto>.Fail("Không tìm thấy giảng viên."))
            : Ok(ApiResponse<GiangVienDto>.Ok(result));
    }

    // STU-05: Thêm giảng viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<GiangVienDto>>> Create([FromBody] GiangVienRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<GiangVienDto>.Ok(result, "Đã thêm giảng viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<GiangVienDto>.Fail(ex.Message));
        }
    }

    // STU-05: Cập nhật giảng viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<GiangVienDto>>> Update(int id, [FromBody] GiangVienRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<GiangVienDto>.Fail("Không tìm thấy giảng viên."))
                : Ok(ApiResponse<GiangVienDto>.Ok(result, "Đã cập nhật giảng viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<GiangVienDto>.Fail(ex.Message));
        }
    }

    // STU-05: Xóa giảng viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            return success
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa giảng viên."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy giảng viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
