using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

[ApiController]
[Route("api/diem")]
[Tags("Grades")]
public class GradeController(IGradeService service) : ControllerBase
{
    private readonly IGradeService _service = service;

    // GRD-01 Nhập điểm theo lớp học phần
    [HttpPut("lop-hoc-phan/{lopHocPhanId:int}")]
    public async Task<IActionResult> NhapDiem(int lopHocPhanId, [FromBody] NhapDiemRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.NhapDiemAsync(lopHocPhanId, request)));

    // GRD-02 Tính điểm tổng kết, điểm chữ
    [HttpPost("lop-hoc-phan/{lopHocPhanId:int}/tinh-diem")]
    public async Task<IActionResult> TinhDiem(int lopHocPhanId)
        => Ok(ApiResponse<object>.Ok(await _service.TinhDiemAsync(lopHocPhanId)));

    // GRD-03 Khóa điểm
    [HttpPost("lop-hoc-phan/{lopHocPhanId:int}/khoa")]
    public async Task<IActionResult> KhoaDiem(int lopHocPhanId)
        => Ok(ApiResponse<object>.Ok(await _service.KhoaDiemAsync(lopHocPhanId)));

    // GRD-04 Bảng điểm sinh viên, GPA
    [HttpGet("sinh-vien/{sinhVienId:int}")]
    public async Task<IActionResult> GetBangDiem(int sinhVienId)
        => Ok(ApiResponse<object>.Ok(await _service.GetBangDiemAsync(sinhVienId)));

    // GRD-05 Gửi / xử lý phúc khảo
    [HttpPost("/api/phuc-khao")]
    public async Task<IActionResult> CreatePhucKhao([FromBody] PhucKhaoRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreatePhucKhaoAsync(request)));
}
