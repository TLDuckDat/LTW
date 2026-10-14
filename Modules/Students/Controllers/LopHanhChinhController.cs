using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

[ApiController]
[Route("api/lop-hanh-chinh")]
[Tags("Students - LopHanhChinh")]
public class LopHanhChinhController(ILopHanhChinhService service) : ControllerBase
{
    private readonly ILopHanhChinhService _service = service;

    // STU-04: Danh sách lớp hành chính
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LopHanhChinhDto>>>> GetPaged(
        [FromQuery] PagedQuery query,
        [FromQuery] int? nganhId,
        [FromQuery] int? khoaTuyen)
        => Ok(ApiResponse<PagedResult<LopHanhChinhDto>>.Ok(await _service.GetPagedAsync(query, nganhId, khoaTuyen)));

    // STU-04: Chi tiết lớp hành chính kèm danh sách sinh viên
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LopHanhChinhDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<LopHanhChinhDetailDto>.Fail("Không tìm thấy lớp hành chính."))
            : Ok(ApiResponse<LopHanhChinhDetailDto>.Ok(result));
    }

    // STU-04: Tạo lớp hành chính
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LopHanhChinhDto>>> Create([FromBody] LopHanhChinhRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<LopHanhChinhDto>.Ok(result, "Đã tạo lớp hành chính."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LopHanhChinhDto>.Fail(ex.Message));
        }
    }

    // STU-04: Sửa lớp hành chính
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<LopHanhChinhDto>>> Update(int id, [FromBody] LopHanhChinhRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<LopHanhChinhDto>.Fail("Không tìm thấy lớp hành chính."))
                : Ok(ApiResponse<LopHanhChinhDto>.Ok(result, "Đã cập nhật lớp hành chính."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LopHanhChinhDto>.Fail(ex.Message));
        }
    }

    // STU-04: Xóa lớp hành chính
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            return success
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa lớp hành chính."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp hành chính."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // STU-04: Phân nhiều sinh viên vào lớp
    [HttpPut("{id:int}/phan-lop")]
    public async Task<ActionResult<ApiResponse<object>>> PhanLop(int id, [FromBody] PhanLopRequest request)
    {
        var success = await _service.PhanLopAsync(id, request);
        return success
            ? Ok(ApiResponse<object>.Ok(null, "Đã phân bổ sinh viên vào lớp."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp hành chính."));
    }

    // STU-05: Gán cố vấn học tập cho lớp
    [HttpPut("{id:int}/phan-co-van")]
    public async Task<ActionResult<ApiResponse<object>>> PhanCoVan(int id, [FromBody] PhanCoVanRequest request)
    {
        try
        {
            var success = await _service.PhanCoVanAsync(id, request);
            return success
                ? Ok(ApiResponse<object>.Ok(null, "Đã cập nhật cố vấn học tập."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp hành chính."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
