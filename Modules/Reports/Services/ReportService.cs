using StudentManagement.Common;
using StudentManagement.Data;

namespace StudentManagement.Modules.Reports.Services;

public class ReportService(AppDbContext db) : IReportService
{
    private readonly AppDbContext _db = db;

    // RPT-01 Thống kê sinh viên theo khoa/ngành/trạng thái
    public Task<object?> ThongKeSinhVienAsync() => throw new NotImplementedException("TODO");

    // RPT-02 Thống kê kết quả học tập theo học kỳ
    public Task<object?> ThongKeKetQuaAsync(int hocKyId) => throw new NotImplementedException("TODO");

    // RPT-03 Báo cáo công nợ học phí
    public Task<object?> BaoCaoCongNoAsync(int hocKyId) => throw new NotImplementedException("TODO");

    // RPT-04 Danh sách sinh viên cảnh báo học vụ
    public Task<object?> CanhBaoHocVuAsync(int hocKyId) => throw new NotImplementedException("TODO");

    // RPT-05 CRUD thông báo
    public Task<object?> GetThongBaoAsync(PagedQuery query) => throw new NotImplementedException("TODO");
}
