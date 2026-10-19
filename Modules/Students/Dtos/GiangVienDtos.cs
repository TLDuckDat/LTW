using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Students.Dtos;

/// <summary>DTO giảng viên / cố vấn học tập</summary>
public record GiangVienDto(
    int Id,
    string Ma,
    string HoTen,
    string? HocVi,
    string? Email,
    string? SoDienThoai,
    int BoMonId,
    string? TenBoMon,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>Request tạo/sửa giảng viên</summary>
public record GiangVienRequest
{
    [Required(ErrorMessage = "Mã giảng viên là bắt buộc.")]
    [MaxLength(20)]
    public string Ma { get; init; } = string.Empty;

    [Required(ErrorMessage = "Họ tên giảng viên là bắt buộc.")]
    [MaxLength(100)]
    public string HoTen { get; init; } = string.Empty;

    [MaxLength(50)]
    public string? HocVi { get; init; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; init; }

    [Phone]
    [MaxLength(20)]
    public string? SoDienThoai { get; init; }

    [Required(ErrorMessage = "Bộ môn là bắt buộc.")]
    public int BoMonId { get; init; }
}

/// <summary>Query lọc giảng viên</summary>
public class GiangVienFilterQuery : PagedQuery
{
    public int? BoMonId { get; set; }
}
