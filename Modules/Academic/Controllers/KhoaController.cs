using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

/// <summary>MẪU CRUD: Controller chỉ gọi Service và bọc kết quả trong ApiResponse.</summary>
[ApiController]
[Route("api/khoa")]
[Tags("Academic")]
public class KhoaController(IKhoaService service) : ControllerBase
{
    private readonly IKhoaService _service = service;

    // ACA-01 Danh sách khoa (phân trang, tìm kiếm theo mã/tên)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<KhoaDto>>>> GetPaged([FromQuery] PagedQuery query)
        => Ok(ApiResponse<PagedResult<KhoaDto>>.Ok(await _service.GetPagedAsync(query)));

    // ACA-01 Chi tiết khoa
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<KhoaDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<KhoaDto>.Fail("Không tìm thấy khoa."))
            : Ok(ApiResponse<KhoaDto>.Ok(result));
    }

    // ACA-01 Thêm khoa
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<KhoaDto>>> Create([FromBody] KhoaRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<KhoaDto>.Ok(result, "Đã tạo khoa."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<KhoaDto>.Fail(ex.Message));
        }
    }

    // ACA-01 Cập nhật khoa
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<KhoaDto>>> Update(int id, [FromBody] KhoaRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<KhoaDto>.Fail("Không tìm thấy khoa."))
                : Ok(ApiResponse<KhoaDto>.Ok(result, "Đã cập nhật khoa."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<KhoaDto>.Fail(ex.Message));
        }
    }

    // ACA-01 Xóa khoa
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa khoa."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy khoa."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
