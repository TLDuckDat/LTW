using System.ComponentModel.DataAnnotations;
using StudentManagement.Modules.Auth.Entities;

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
    DateTime NgayTao,
    int? SinhVienId,
    int? GiangVienId);

/// <summary>Admin tạo tài khoản; SinhVienId/GiangVienId chỉ dùng cho vai trò tương ứng.</summary>
public record CreateAccountRequest(
    [Required, StringLength(50)] string TenDangNhap,
    [Required, MinLength(8), StringLength(72)] string MatKhau,
    [Required, StringLength(100)] string HoTen,
    [Required, EnumDataType(typeof(VaiTro))] VaiTro? VaiTro,
    int? SinhVienId = null,
    int? GiangVienId = null);

/// <summary>Admin đặt lại mật khẩu cho tài khoản.</summary>
public record ResetPasswordRequest(
    [Required, MinLength(8), StringLength(72)] string MatKhauMoi);

/// <summary>Admin đổi vai trò tài khoản.</summary>
public record ChangeRoleRequest(
    [Required, EnumDataType(typeof(VaiTro))] VaiTro? VaiTro);

/// <summary>Liên kết tài khoản với hồ sơ sinh viên; SinhVienId = null để gỡ liên kết.</summary>
public record LinkStudentRequest(int? SinhVienId);

/// <summary>Liên kết tài khoản với hồ sơ giảng viên; GiangVienId = null để gỡ liên kết.</summary>
public record LinkLecturerRequest(int? GiangVienId);

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
