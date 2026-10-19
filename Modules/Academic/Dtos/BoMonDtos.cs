using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Dtos;

public record BoMonDto(
    int Id,
    string Ma,
    string Ten,
    int KhoaId,
    string? TenKhoa,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record BoMonRequest(
    [Required(ErrorMessage = "Mã bộ môn là bắt buộc.")]
    [MaxLength(20, ErrorMessage = "Mã bộ môn tối đa 20 ký tự.")]
    string Ma,

    [Required(ErrorMessage = "Tên bộ môn là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Tên bộ môn tối đa 200 ký tự.")]
    string Ten,

    [Required(ErrorMessage = "KhoaId là bắt buộc.")]
    int KhoaId
);

public class BoMonFilterQuery : PagedQuery
{
    public int? KhoaId { get; set; }
}
