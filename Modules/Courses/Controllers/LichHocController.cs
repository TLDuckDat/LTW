using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[Route("api/lop-hoc-phan/{lopHocPhanId:int}/lich-hoc")]
[Tags("Courses")]
public class LichHocController(ILichHocService service) : ApiControllerBase
{
    // CRS-05 Thời khóa biểu của lớp học phần
    [HttpGet]
    public Task<ActionResult<ApiResponse<IReadOnlyList<LichHocDto>>>> GetByLop(int lopHocPhanId) =>
        Execute(() => service.GetByLopAsync(lopHocPhanId));

    // CRS-05 Xếp buổi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<LichHocDto>>> Add(int lopHocPhanId, [FromBody] LichHocRequest request) =>
        Execute(() => service.AddAsync(lopHocPhanId, request), StatusCodes.Status201Created, "Đã xếp lịch học.");

    // CRS-05 Sửa buổi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{lichHocId:int}")]
    public Task<ActionResult<ApiResponse<LichHocDto>>> Update(int lopHocPhanId, int lichHocId, [FromBody] LichHocRequest request) =>
        Execute(() => service.UpdateAsync(lopHocPhanId, lichHocId, request), message: "Đã cập nhật lịch học.");

    // CRS-06 Kiểm tra xung đột phòng/giảng viên trước khi xếp (lichHocId: buổi đang sửa, nếu có)
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("kiem-tra-xung-dot")]
    public Task<ActionResult<ApiResponse<KiemTraXungDotDto>>> KiemTraXungDot(
        int lopHocPhanId, [FromBody] LichHocRequest request, [FromQuery] int? lichHocId) =>
        Execute(() => service.KiemTraXungDotAsync(lopHocPhanId, request, lichHocId));

    // CRS-05 Xóa buổi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{lichHocId:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int lopHocPhanId, int lichHocId) =>
        Execute(() => service.DeleteAsync(lopHocPhanId, lichHocId), "Đã xóa buổi học.");
}
