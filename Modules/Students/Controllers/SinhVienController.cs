using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

[ApiController]
[Route("api/sinh-vien")]
[Tags("Students")]
public class SinhVienController(ISinhVienService service) : ControllerBase
{
    private readonly ISinhVienService _service = service;

    // STU-01 Danh sách sinh viên (phân trang, lọc)
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetPagedAsync(query)));

    // STU-02 Xem hồ sơ sinh viên
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<object>.Ok(await _service.GetByIdAsync(id)));

    // STU-03 Thêm sinh viên
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SinhVienRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateAsync(request)));

    // STU-04 Cập nhật sinh viên
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SinhVienRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.UpdateAsync(id, request)));

    // STU-05 Đổi trạng thái học tập
    [HttpPut("{id:int}/trang-thai")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.ChangeStatusAsync(id, request)));

    // STU-06 CRUD giảng viên
    [HttpGet("/api/giang-vien")]
    public async Task<IActionResult> GetGiangVien([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetGiangVienAsync(query)));

    // STU-07 CRUD lớp hành chính, phân cố vấn
    [HttpGet("/api/lop-hanh-chinh")]
    public async Task<IActionResult> GetLopHanhChinh([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetLopHanhChinhAsync(query)));
}
