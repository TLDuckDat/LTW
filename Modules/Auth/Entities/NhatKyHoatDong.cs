using StudentManagement.Common;

namespace StudentManagement.Modules.Auth.Entities;

/// <summary>Nhật ký hoạt động liên quan đến tài khoản.</summary>
public class NhatKyHoatDong : BaseEntity
{
    public int? TaiKhoanId { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string HanhDong { get; set; } = string.Empty;
    public bool ThanhCong { get; set; }
    public string? DiaChiIp { get; set; }
    public string? UserAgent { get; set; }
}
