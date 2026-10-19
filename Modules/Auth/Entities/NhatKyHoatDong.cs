using StudentManagement.Common;

namespace StudentManagement.Modules.Auth.Entities;

/// <summary>
/// Nhật ký hoạt động: đăng nhập/làm mới phiên (ghi trong AuthService) và mọi thao tác ghi dữ liệu
/// của người đã đăng nhập (ghi tự động bởi NhatKyHoatDongMiddleware). Không bao giờ lưu body request.
/// </summary>
public class NhatKyHoatDong : BaseEntity
{
    public int? TaiKhoanId { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    /// <summary>"Login", "Refresh"... hoặc "{Controller}.{Action}" với thao tác ghi, ví dụ "SinhVien.Delete".</summary>
    public string HanhDong { get; set; } = string.Empty;
    public bool ThanhCong { get; set; }
    public string? PhuongThuc { get; set; }
    public string? DuongDan { get; set; }
    public int? MaPhanHoi { get; set; }
    /// <summary>Tham số đường dẫn/query và chi tiết nghiệp vụ (ví dụ "vai trò SinhVien -> GiangVien").</summary>
    public string? ChiTiet { get; set; }
    public string? DiaChiIp { get; set; }
    public string? UserAgent { get; set; }
}
