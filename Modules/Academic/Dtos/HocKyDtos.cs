using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Dtos;

public record HocKyDto(
    int Id,
    string Ten,
    int SoThuTu,
    int NamHocId,
    string? TenNamHoc,
    DateOnly NgayBatDau,
    DateOnly NgayKetThuc,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record HocKyRequest(
    [Required(ErrorMessage = "Tên học kỳ là bắt buộc (ví dụ: Học kỳ 1).")]
    [MaxLength(50, ErrorMessage = "Tên học kỳ tối đa 50 ký tự.")]
    string Ten,

    [Required(ErrorMessage = "Số thứ tự học kỳ là bắt buộc.")]
    [Range(1, 10, ErrorMessage = "Số thứ tự học kỳ từ 1 đến 10.")]
    int SoThuTu,

    [Required(ErrorMessage = "NamHocId là bắt buộc.")]
    int NamHocId,

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    DateOnly NgayBatDau,

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    DateOnly NgayKetThuc
);

public class HocKyFilterQuery : PagedQuery
{
    public int? NamHocId { get; set; }
}
