using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[ApiController]
[Route("api/tot-nghiep")]
[Tags("Graduation")]
public class GraduationController(IGraduationService service) : ControllerBase
{
    private readonly IGraduationService _service = service;

    // GRAD-01 Đăng ký / chấm thực tập
    [HttpPost("thuc-tap")]
    public async Task<IActionResult> CreateThucTap([FromBody] ThucTapRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateThucTapAsync(request)));

    // GRAD-02 Đăng ký đề tài khóa luận
    [HttpPost("de-tai")]
    public async Task<IActionResult> CreateDeTai([FromBody] DeTaiRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateDeTaiAsync(request)));

    // GRAD-03 Lập hội đồng bảo vệ
    [HttpPost("hoi-dong")]
    public async Task<IActionResult> CreateHoiDong([FromBody] HoiDongRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateHoiDongAsync(request)));

    // GRAD-04 Xét tốt nghiệp
    [HttpPost("xet/{sinhVienId:int}")]
    public async Task<IActionResult> XetTotNghiep(int sinhVienId)
        => Ok(ApiResponse<object>.Ok(await _service.XetTotNghiepAsync(sinhVienId)));
}
