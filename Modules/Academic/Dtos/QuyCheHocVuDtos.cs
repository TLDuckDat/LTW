using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Dtos;

public record QuyCheHocVuDto(
    int Id,
    string Ma,
    string Ten,
    string GiaTri,
    string? DonVi,
    string? MoTa,
    int? ApDungTuKhoa,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record QuyCheHocVuRequest(
    [Required(ErrorMessage = "Mã quy chế là bắt buộc (ví dụ: MAX_TIN_CHI_KY).")]
    [MaxLength(50, ErrorMessage = "Mã quy chế tối đa 50 ký tự.")]
    string Ma,

    [Required(ErrorMessage = "Tên quy chế là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Tên quy chế tối đa 200 ký tự.")]
    string Ten,

    [Required(ErrorMessage = "Giá trị là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Giá trị tối đa 100 ký tự.")]
    string GiaTri,

    [MaxLength(50, ErrorMessage = "Đơn vị tính tối đa 50 ký tự.")]
    string? DonVi = null,

    [MaxLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    string? MoTa = null,

    int? ApDungTuKhoa = null
);

public class QuyCheHocVuFilterQuery : PagedQuery
{
    public int? ApDungTuKhoa { get; set; }
}
