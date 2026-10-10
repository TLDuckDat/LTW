using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Auth.Dtos;

/// <summary>Yêu cầu đăng nhập.</summary>
public record LoginRequest(
    [Required, StringLength(50)] string TenDangNhap,
    [Required, StringLength(72)] string MatKhau);

/// <summary>Yêu cầu thay đổi mật khẩu.</summary>
public record ChangePasswordRequest(
    [Required, StringLength(72)] string MatKhauCu,
    [Required, MinLength(8), StringLength(72)] string MatKhauMoi);

/// <summary>Phản hồi đăng nhập.</summary>
public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    int TaiKhoanId,
    string TenDangNhap,
    string HoTen,
    string VaiTro);

/// <summary>Phản hồi thông tin tài khoản.</summary>
public record AccountDto(
    int Id,
    string TenDangNhap,
    string HoTen,
    string VaiTro,
    bool DaKhoa,
    DateTime NgayTao);

/// <summary>Thông tin một hoạt động được ghi nhận.</summary>
public record ActivityLogDto(
    int Id,
    int? TaiKhoanId,
    string TenDangNhap,
    string HanhDong,
    bool ThanhCong,
    string? DiaChiIp,
    string? UserAgent,
    DateTime ThoiGian);
