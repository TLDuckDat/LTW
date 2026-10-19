using StudentManagement.Common;

namespace StudentManagement.Modules.Auth.Entities;

/// <summary>
/// Refresh token của một phiên đăng nhập. Chỉ lưu hash SHA-256, không lưu token gốc.
/// Mỗi lần làm mới, token cũ bị thu hồi (LyDoThuHoi = XoayVong) và cấp token mới.
/// </summary>
public class RefreshToken : BaseEntity
{
    public int TaiKhoanId { get; set; } // -> TaiKhoan
    public string TokenHash { get; set; } = string.Empty;
    public DateTime HetHan { get; set; }
    public DateTime? ThuHoiLuc { get; set; }
    public string? LyDoThuHoi { get; set; }
    public string? DiaChiIp { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>Lý do thu hồi refresh token.</summary>
public static class LyDoThuHoiToken
{
    /// <summary>Đã đổi lấy token mới; dùng lại token này là dấu hiệu bị đánh cắp.</summary>
    public const string XoayVong = "XoayVong";
    public const string DangXuat = "DangXuat";
    public const string DangXuatTatCa = "DangXuatTatCa";
    /// <summary>Phát hiện token đã xoay vòng bị dùng lại: thu hồi mọi phiên của tài khoản.</summary>
    public const string NghiBiDanhCap = "NghiBiDanhCap";
    /// <summary>Tài khoản bị khóa, đổi mật khẩu hoặc đổi vai trò sau khi token được cấp.</summary>
    public const string TaiKhoanThayDoi = "TaiKhoanThayDoi";
}
