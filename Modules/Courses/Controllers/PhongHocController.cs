using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[ApiController]
[Route("api/phong-hoc")]
[Tags("Courses")]
public class PhongHocController(IPhongHocService service) : ControllerBase
{
    private readonly IPhongHocService _service = service;

    // CRS-04 Danh sách phòng học
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PhongHocDto>>>> GetPaged([FromQuery] PagedQuery query)
        => Ok(ApiResponse<PagedResult<PhongHocDto>>.Ok(await _service.GetPagedAsync(query)));

    // CRS-04 Chi tiết phòng học
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PhongHocDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<PhongHocDto>.Fail("Không tìm thấy phòng học."))
            : Ok(ApiResponse<PhongHocDto>.Ok(result));
    }

    // CRS-04 Thêm phòng học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PhongHocDto>>> Create([FromBody] PhongHocRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PhongHocDto>.Ok(result, "Đã tạo phòng học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PhongHocDto>.Fail(ex.Message));
        }
    }

    // CRS-04 Cập nhật phòng học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PhongHocDto>>> Update(int id, [FromBody] PhongHocRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<PhongHocDto>.Fail("Không tìm thấy phòng học."))
                : Ok(ApiResponse<PhongHocDto>.Ok(result, "Đã cập nhật phòng học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PhongHocDto>.Fail(ex.Message));
        }
    }

    // CRS-04 Xóa phòng học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa phòng học."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy phòng học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
