using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Reports.Services;

namespace StudentManagement.Modules.Reports.Controllers;

[ApiController]
[Route("api/bao-cao")]
[Tags("Reports")]
public class ReportController(IReportService service) : ControllerBase
{
    private readonly IReportService _service = service;

    // RPT-01 Thống kê sinh viên theo khoa/ngành/trạng thái
    [HttpGet("sinh-vien")]
    public async Task<IActionResult> ThongKeSinhVien()
        => Ok(ApiResponse<object>.Ok(await _service.ThongKeSinhVienAsync()));

    // RPT-02 Thống kê kết quả học tập theo học kỳ
    [HttpGet("ket-qua-hoc-tap/{hocKyId:int}")]
    public async Task<IActionResult> ThongKeKetQua(int hocKyId)
        => Ok(ApiResponse<object>.Ok(await _service.ThongKeKetQuaAsync(hocKyId)));

    // RPT-03 Báo cáo công nợ học phí
    [HttpGet("cong-no/{hocKyId:int}")]
    public async Task<IActionResult> BaoCaoCongNo(int hocKyId)
        => Ok(ApiResponse<object>.Ok(await _service.BaoCaoCongNoAsync(hocKyId)));

    // RPT-04 Danh sách sinh viên cảnh báo học vụ
    [HttpGet("canh-bao-hoc-vu/{hocKyId:int}")]
    public async Task<IActionResult> CanhBaoHocVu(int hocKyId)
        => Ok(ApiResponse<object>.Ok(await _service.CanhBaoHocVuAsync(hocKyId)));

    // RPT-05 CRUD thông báo
    [HttpGet("/api/thong-bao")]
    public async Task<IActionResult> GetThongBao([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetThongBaoAsync(query)));
}
