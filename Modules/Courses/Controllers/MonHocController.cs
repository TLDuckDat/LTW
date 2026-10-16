using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[ApiController]
[Route("api/mon-hoc")]
[Tags("Courses")]
public class MonHocController(IMonHocService service) : ControllerBase
{
    private readonly IMonHocService _service = service;

    // CRS-01 Danh sách môn học (phân trang, tìm kiếm theo mã/tên)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MonHocDto>>>> GetPaged([FromQuery] PagedQuery query)
        => Ok(ApiResponse<PagedResult<MonHocDto>>.Ok(await _service.GetPagedAsync(query)));

    // CRS-01 Chi tiết môn học
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<MonHocDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<MonHocDto>.Fail("Không tìm thấy môn học."))
            : Ok(ApiResponse<MonHocDto>.Ok(result));
    }

    // CRS-01 Thêm môn học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MonHocDto>>> Create([FromBody] MonHocRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<MonHocDto>.Ok(result, "Đã tạo môn học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<MonHocDto>.Fail(ex.Message));
        }
    }

    // CRS-01 Cập nhật môn học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<MonHocDto>>> Update(int id, [FromBody] MonHocRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<MonHocDto>.Fail("Không tìm thấy môn học."))
                : Ok(ApiResponse<MonHocDto>.Ok(result, "Đã cập nhật môn học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<MonHocDto>.Fail(ex.Message));
        }
    }

    // CRS-01 Xóa môn học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa môn học."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy môn học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // CRS-01 Danh sách môn tiên quyết của một môn học
    [HttpGet("{id:int}/tien-quyet")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MonHocDto>>>> GetTienQuyet(int id)
    {
        var result = await _service.GetTienQuyetAsync(id);
        return result is null
            ? NotFound(ApiResponse<IReadOnlyList<MonHocDto>>.Fail("Không tìm thấy môn học."))
            : Ok(ApiResponse<IReadOnlyList<MonHocDto>>.Ok(result));
    }

    // CRS-01 Thêm môn tiên quyết
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/tien-quyet")]
    public async Task<ActionResult<ApiResponse<MonHocDto>>> AddTienQuyet(int id, [FromBody] TienQuyetRequest request)
    {
        try
        {
            var result = await _service.AddTienQuyetAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<MonHocDto>.Fail("Không tìm thấy môn học hoặc môn tiên quyết."))
                : StatusCode(StatusCodes.Status201Created, ApiResponse<MonHocDto>.Ok(result, "Đã thêm môn tiên quyết."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<MonHocDto>.Fail(ex.Message));
        }
    }

    // CRS-01 Gỡ môn tiên quyết
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}/tien-quyet/{tienQuyetId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveTienQuyet(int id, int tienQuyetId)
        => await _service.RemoveTienQuyetAsync(id, tienQuyetId)
            ? Ok(ApiResponse<object>.Ok(null, "Đã gỡ môn tiên quyết."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy quan hệ tiên quyết."));
}
