using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Services;

namespace StudentManagement.Modules.Registration.Controllers;

[ApiController]
[Route("api/dang-ky-hoc-phan")]
[Tags("Registration")]
public class RegistrationController(IRegistrationService service) : ControllerBase
{
    private readonly IRegistrationService _service = service;

    // REG-01 Quản lý đợt đăng ký
    [HttpPost("/api/dot-dang-ky")]
    public async Task<IActionResult> CreateDotDangKy([FromBody] DotDangKyRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateDotDangKyAsync(request)));

    // REG-02 Danh sách lớp học phần mở cho sinh viên
    [HttpGet("lop-mo")]
    public async Task<IActionResult> GetLopMo(int sinhVienId)
        => Ok(ApiResponse<object>.Ok(await _service.GetLopMoAsync(sinhVienId)));

    // REG-03 Đăng ký học phần (kiểm tra tiên quyết, sĩ số, trùng lịch)
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] DangKyRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.RegisterAsync(request)));

    // REG-04 Hủy đăng ký học phần
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(ApiResponse<object>.Ok(await _service.CancelAsync(id)));

    // REG-05 Xem thời khóa biểu sinh viên
    [HttpGet("thoi-khoa-bieu/{sinhVienId:int}")]
    public async Task<IActionResult> GetThoiKhoaBieu(int sinhVienId)
        => Ok(ApiResponse<object>.Ok(await _service.GetThoiKhoaBieuAsync(sinhVienId)));
}
