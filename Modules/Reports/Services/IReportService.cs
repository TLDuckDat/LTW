using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Reports.Dtos;

namespace StudentManagement.Modules.Reports.Services;

/// <summary>Nghiệp vụ module Reports (Epic RPT). Báo cáo chỉ đọc dữ liệu, không có bảng riêng.</summary>
public interface IReportService
{
    // RPT-01 Thống kê sinh viên theo khoa/ngành/trạng thái
    Task<ThongKeSinhVienDto> ThongKeSinhVienAsync();
    // RPT-02 Thống kê kết quả học tập theo học kỳ
    Task<ThongKeKetQuaHocTapDto> ThongKeKetQuaAsync(int hocKyId);
    // RPT-03 Báo cáo công nợ học phí
    Task<BaoCaoCongNoDto> BaoCaoCongNoAsync(int hocKyId, PagedQuery query);
    // RPT-05 Số liệu tổng quan cho Dashboard (một lần gọi thay cho nhiều thống kê)
    Task<TongQuanDto> TongQuanAsync();
    // RPT-04 Danh sách sinh viên cảnh báo học vụ
    Task<PagedResult<CanhBaoHocVuDto>> CanhBaoHocVuAsync(int hocKyId, PagedQuery query);
}
