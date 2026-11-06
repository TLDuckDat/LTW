using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Graduation.Dtos;

public record CoSoThucTapDto(int Id, string Ma, string Ten, string? DiaChi,
    string? SoDienThoai, string? Email, string? NguoiLienHe, bool DangTiepNhan);

public record CoSoThucTapRequest(
    [Required, MaxLength(30), RegularExpression(@"^[A-Za-z0-9_-]{2,30}$", ErrorMessage = "Mã chỉ gồm chữ, số, '-' hoặc '_' (2-30 ký tự).")] string Ma,
    [Required, MinLength(2), MaxLength(200)] string Ten,
    [MaxLength(300)] string? DiaChi,
    [MaxLength(20), RegularExpression(@"^(0\d{9,10}|1[89]00\d{4,6})$", ErrorMessage = "Số điện thoại không hợp lệ.")] string? SoDienThoai,
    [EmailAddress, MaxLength(100)] string? Email,
    [MaxLength(100), RegularExpression(@"^[\p{L}\s'.-]{2,100}$", ErrorMessage = "Người liên hệ chỉ gồm chữ cái.")] string? NguoiLienHe);

public record TrangThaiCoSoRequest(bool DangTiepNhan);
