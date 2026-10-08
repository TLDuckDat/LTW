using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[ApiController]
[Route("api/mon-hoc")]
[Tags("Courses")]
public class CourseController(ICourseService service) : ControllerBase
{
    private readonly ICourseService _service = service;

    // CRS-01 CRUD môn học
    [HttpGet]
    public async Task<IActionResult> GetMonHoc([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetMonHocAsync(query)));

    // CRS-02 Quản lý môn tiên quyết
    [HttpPost("{id:int}/tien-quyet")]
    public async Task<IActionResult> AddTienQuyet(int id, [FromBody] TienQuyetRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.AddTienQuyetAsync(id, request)));

    // CRS-03 CRUD phòng học
    [HttpGet("/api/phong-hoc")]
    public async Task<IActionResult> GetPhongHoc([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetPhongHocAsync(query)));

    // CRS-04 Mở lớp học phần
    [HttpPost("/api/lop-hoc-phan")]
    public async Task<IActionResult> CreateLopHocPhan([FromBody] LopHocPhanRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateLopHocPhanAsync(request)));

    // CRS-05 Xếp lịch học, kiểm tra trùng phòng/giờ
    [HttpPost("/api/lop-hoc-phan/{id:int}/lich-hoc")]
    public async Task<IActionResult> AddLichHoc(int id, [FromBody] LichHocRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.AddLichHocAsync(id, request)));
}
