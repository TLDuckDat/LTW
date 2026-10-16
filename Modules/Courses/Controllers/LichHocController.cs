using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[ApiController]
[Route("api/lop-hoc-phan/{lopHocPhanId:int}/lich-hoc")]
[Tags("Courses")]
public class LichHocController(ILichHocService service) : ControllerBase
{
    private readonly ILichHocService _service = service;

    // CRS-05 Thời khóa biểu của lớp học phần
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LichHocDto>>>> GetByLop(int lopHocPhanId)
    {
        var result = await _service.GetByLopAsync(lopHocPhanId);
        return result is null
            ? NotFound(ApiResponse<IReadOnlyList<LichHocDto>>.Fail("Không tìm thấy lớp học phần."))
            : Ok(ApiResponse<IReadOnlyList<LichHocDto>>.Ok(result));
    }

    // CRS-05 Xếp buổi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LichHocDto>>> Add(int lopHocPhanId, [FromBody] LichHocRequest request)
    {
        try
        {
            var result = await _service.AddAsync(lopHocPhanId, request);
            return result is null
                ? NotFound(ApiResponse<LichHocDto>.Fail("Không tìm thấy lớp học phần."))
                : StatusCode(StatusCodes.Status201Created, ApiResponse<LichHocDto>.Ok(result, "Đã xếp lịch học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LichHocDto>.Fail(ex.Message));
        }
    }

    // CRS-05 Sửa buổi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{lichHocId:int}")]
    public async Task<ActionResult<ApiResponse<LichHocDto>>> Update(int lopHocPhanId, int lichHocId, [FromBody] LichHocRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(lopHocPhanId, lichHocId, request);
            return result is null
                ? NotFound(ApiResponse<LichHocDto>.Fail("Không tìm thấy lớp học phần hoặc buổi học."))
                : Ok(ApiResponse<LichHocDto>.Ok(result, "Đã cập nhật lịch học."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<LichHocDto>.Fail(ex.Message));
        }
    }

    // CRS-05 Xóa buổi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{lichHocId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int lopHocPhanId, int lichHocId)
        => await _service.DeleteAsync(lopHocPhanId, lichHocId)
            ? Ok(ApiResponse<object>.Ok(null, "Đã xóa buổi học."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy buổi học."));
}
