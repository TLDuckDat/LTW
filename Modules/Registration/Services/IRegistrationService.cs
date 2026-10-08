using StudentManagement.Modules.Registration.Dtos;

namespace StudentManagement.Modules.Registration.Services;

/// <summary>Nghiệp vụ module Registration (Epic REG).</summary>
public interface IRegistrationService
{
    // REG-01 Quản lý đợt đăng ký
    Task<object?> CreateDotDangKyAsync(DotDangKyRequest request);
    // REG-02 Danh sách lớp học phần mở cho sinh viên
    Task<object?> GetLopMoAsync(int sinhVienId);
    // REG-03 Đăng ký học phần (kiểm tra tiên quyết, sĩ số, trùng lịch)
    Task<object?> RegisterAsync(DangKyRequest request);
    // REG-04 Hủy đăng ký học phần
    Task<object?> CancelAsync(int id);
    // REG-05 Xem thời khóa biểu sinh viên
    Task<object?> GetThoiKhoaBieuAsync(int sinhVienId);
}
