using StudentManagement.Data;
using StudentManagement.Modules.StudentAffairs.Dtos;

namespace StudentManagement.Modules.StudentAffairs.Services;

public class StudentAffairsService(AppDbContext db) : IStudentAffairsService
{
    private readonly AppDbContext _db = db;

    // SA-01 Nhập điểm rèn luyện
    public Task<object?> NhapDiemRenLuyenAsync(DiemRenLuyenRequest request) => throw new NotImplementedException("TODO");

    // SA-02 Tính học phí theo học kỳ
    public Task<object?> TinhHocPhiAsync(int hocKyId) => throw new NotImplementedException("TODO");

    // SA-03 Xác nhận thanh toán học phí
    public Task<object?> ThanhToanAsync(int id) => throw new NotImplementedException("TODO");

    // SA-04 Quản lý miễn giảm
    public Task<object?> CreateMienGiamAsync(MienGiamRequest request) => throw new NotImplementedException("TODO");

    // SA-05 Xét học bổng
    public Task<object?> XetHocBongAsync(int hocKyId) => throw new NotImplementedException("TODO");

    // SA-06 Khen thưởng / kỷ luật
    public Task<object?> CreateKhenThuongKyLuatAsync(KhenThuongKyLuatRequest request) => throw new NotImplementedException("TODO");
}
