namespace StudentManagement.Modules.Auth.Services;

/// <summary>
/// Thông tin tài khoản đang đăng nhập, dùng cho các API tự phục vụ ("/toi") của sinh viên và giảng viên.
/// Các hàm Get...Async báo lỗi 403 nếu tài khoản chưa được liên kết với hồ sơ tương ứng.
/// </summary>
public interface ICurrentUserService
{
    int AccountId { get; }
    bool IsInRole(string role);
    Task<int> GetSinhVienIdAsync();
    Task<int> GetGiangVienIdAsync();

    /// <summary>Id giảng viên nếu tài khoản có vai trò GiangVien; null với Admin/PhongDaoTao (không giới hạn lớp).</summary>
    Task<int?> GetGiangVienIdNeuLaGiangVienAsync();
}
