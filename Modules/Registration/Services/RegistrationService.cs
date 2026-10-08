using StudentManagement.Data;
using StudentManagement.Modules.Registration.Dtos;

namespace StudentManagement.Modules.Registration.Services;

public class RegistrationService(AppDbContext db) : IRegistrationService
{
    private readonly AppDbContext _db = db;

    // REG-01 Quản lý đợt đăng ký
    public Task<object?> CreateDotDangKyAsync(DotDangKyRequest request) => throw new NotImplementedException("TODO");

    // REG-02 Danh sách lớp học phần mở cho sinh viên
    public Task<object?> GetLopMoAsync(int sinhVienId) => throw new NotImplementedException("TODO");

    // REG-03 Đăng ký học phần (kiểm tra tiên quyết, sĩ số, trùng lịch)
    public Task<object?> RegisterAsync(DangKyRequest request) => throw new NotImplementedException("TODO");

    // REG-04 Hủy đăng ký học phần
    public Task<object?> CancelAsync(int id) => throw new NotImplementedException("TODO");

    // REG-05 Xem thời khóa biểu sinh viên
    public Task<object?> GetThoiKhoaBieuAsync(int sinhVienId) => throw new NotImplementedException("TODO");
}
