using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[ApiController]
[Route("api/lop-hoc-phan")]
[Tags("Courses")]
public class LopHocPhanController(ILopHocPhanService service) : ControllerBase
{
    private readonly ILopHocPhanService _service = service;

    // CRS-02 Danh sách lớp học phần (lọc theo học kỳ, môn, giảng viên, trạng thái)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LopHocPhanDto>>>> GetPaged([FromQuery] LopHocPhanQuery query)
        => Ok(ApiResponse<PagedResult<LopHocPhanDto>>.Ok(await _service.GetPagedAsync(query)));

    // CRS-02 Chi tiết lớp học phần
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LopHocPhanDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<LopHocPhanDto>.Fail("Không tìm thấy lớp học phần."))
            : Ok(ApiResponse<LopHocPhanDto>.Ok(result));
    }

    // CRS-02 Mở lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LopHocPhanDto>>> Create([FromBody] LopHocPhanRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<LopHocPhanDto>.Ok(result, "Đã mở lớp học phần."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LopHocPhanDto>.Fail(ex.Message));
        }
    }

    // CRS-02 Cập nhật lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<LopHocPhanDto>>> Update(int id, [FromBody] LopHocPhanUpdateRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<LopHocPhanDto>.Fail("Không tìm thấy lớp học phần."))
                : Ok(ApiResponse<LopHocPhanDto>.Ok(result, "Đã cập nhật lớp học phần."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LopHocPhanDto>.Fail(ex.Message));
        }
    }

    // CRS-02 Xóa lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa lớp học phần."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp học phần."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
