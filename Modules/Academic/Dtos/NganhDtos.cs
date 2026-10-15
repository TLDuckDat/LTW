using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Dtos;

public record NganhDto(
    int Id,
    string Ma,
    string Ten,
    int KhoaId,
    string? TenKhoa,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record NganhRequest(
    [Required(ErrorMessage = "Mã ngành là bắt buộc.")]
    [MaxLength(20, ErrorMessage = "Mã ngành tối đa 20 ký tự.")]
    string Ma,

    [Required(ErrorMessage = "Tên ngành là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Tên ngành tối đa 200 ký tự.")]
    string Ten,

    [Required(ErrorMessage = "KhoaId là bắt buộc.")]
    int KhoaId
);

public class NganhFilterQuery : PagedQuery
{
    public int? KhoaId { get; set; }
}
