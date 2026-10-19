using StudentManagement.Common;

namespace StudentManagement.Modules.Auth.Entities;

/// <summary>Tài khoản đăng nhập hệ thống.</summary>
public class TaiKhoan : BaseEntity
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhauHash { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public VaiTro VaiTro { get; set; } = VaiTro.Admin;
    public bool DaKhoa { get; set; }
    public int? SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id); chỉ dùng cho vai trò SinhVien
}
