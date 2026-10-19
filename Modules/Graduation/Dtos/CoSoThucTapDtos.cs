using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Graduation.Dtos;

public record CoSoThucTapDto(int Id, string Ma, string Ten, string? DiaChi,
    string? SoDienThoai, string? Email, string? NguoiLienHe, bool DangTiepNhan);

public record CoSoThucTapRequest(
    [Required, MaxLength(30)] string Ma,
    [Required, MaxLength(200)] string Ten,
    [MaxLength(300)] string? DiaChi,
    [MaxLength(20)] string? SoDienThoai,
    [EmailAddress, MaxLength(100)] string? Email,
    [MaxLength(100)] string? NguoiLienHe);

public record TrangThaiCoSoRequest(bool DangTiepNhan);
