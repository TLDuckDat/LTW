using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Reports.Dtos;
using StudentManagement.Modules.Reports.Services;

namespace StudentManagement.Modules.Reports.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/bao-cao")]
[Tags("Reports")]
public class ReportController(IReportService service) : ApiControllerBase
{
    // RPT-01 Thống kê sinh viên theo khoa/ngành/trạng thái
    [HttpGet("sinh-vien")]
    public Task<ActionResult<ApiResponse<ThongKeSinhVienDto>>> ThongKeSinhVien() =>
        Execute(() => service.ThongKeSinhVienAsync());

    // RPT-05 Số liệu tổng quan cho Dashboard (KPI, phân bố theo khoa, GPA học kỳ mốc)
    [HttpGet("tong-quan")]
    public Task<ActionResult<ApiResponse<TongQuanDto>>> TongQuan() =>
        Execute(() => service.TongQuanAsync());

    // RPT-02 Thống kê kết quả học tập theo học kỳ
    [HttpGet("ket-qua-hoc-tap/{hocKyId:int}")]
    public Task<ActionResult<ApiResponse<ThongKeKetQuaHocTapDto>>> ThongKeKetQua(int hocKyId) =>
        Execute(() => service.ThongKeKetQuaAsync(hocKyId));

    // RPT-03 Báo cáo công nợ học phí (danh sách còn nợ có phân trang, tìm theo MSSV/họ tên)
    [HttpGet("cong-no/{hocKyId:int}")]
    public Task<ActionResult<ApiResponse<BaoCaoCongNoDto>>> BaoCaoCongNo(int hocKyId, [FromQuery] PagedQuery query) =>
        Execute(() => service.BaoCaoCongNoAsync(hocKyId, query));

    // RPT-04 Danh sách sinh viên cảnh báo học vụ
    [HttpGet("canh-bao-hoc-vu/{hocKyId:int}")]
    public Task<ActionResult<ApiResponse<PagedResult<CanhBaoHocVuDto>>>> CanhBaoHocVu(
        int hocKyId, [FromQuery] PagedQuery query) =>
        Execute(() => service.CanhBaoHocVuAsync(hocKyId, query));
}
