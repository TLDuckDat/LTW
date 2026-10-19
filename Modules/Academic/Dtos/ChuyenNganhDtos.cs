using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Dtos;

public record ChuyenNganhDto(
    int Id,
    string Ma,
    string Ten,
    int NganhId,
    string? TenNganh,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ChuyenNganhRequest(
    [Required(ErrorMessage = "Mã chuyên ngành là bắt buộc.")]
    [MaxLength(20, ErrorMessage = "Mã chuyên ngành tối đa 20 ký tự.")]
    string Ma,

    [Required(ErrorMessage = "Tên chuyên ngành là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Tên chuyên ngành tối đa 200 ký tự.")]
    string Ten,

    [Required(ErrorMessage = "NganhId là bắt buộc.")]
    int NganhId
);

public class ChuyenNganhFilterQuery : PagedQuery
{
    public int? NganhId { get; set; }
}
