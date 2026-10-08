using StudentManagement.Common;

namespace StudentManagement.Modules.Reports.Services;

/// <summary>Nghiệp vụ module Reports (Epic RPT).</summary>
public interface IReportService
{
    // RPT-01 Thống kê sinh viên theo khoa/ngành/trạng thái
    Task<object?> ThongKeSinhVienAsync();
    // RPT-02 Thống kê kết quả học tập theo học kỳ
    Task<object?> ThongKeKetQuaAsync(int hocKyId);
    // RPT-03 Báo cáo công nợ học phí
    Task<object?> BaoCaoCongNoAsync(int hocKyId);
    // RPT-04 Danh sách sinh viên cảnh báo học vụ
    Task<object?> CanhBaoHocVuAsync(int hocKyId);
    // RPT-05 CRUD thông báo
    Task<object?> GetThongBaoAsync(PagedQuery query);
}
