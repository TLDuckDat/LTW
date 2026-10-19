using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Dtos;

// DTO cho CTĐT
public record ChuongTrinhDaoTaoDto(
    int Id,
    string Ma,
    string Ten,
    int NganhId,
    string? TenNganh,
    int KhoaTuyen,
    int TongSoTinChi,
    int SoLuongMonHoc,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ChuongTrinhDaoTaoRequest(
    [Required(ErrorMessage = "Mã chương trình đào tạo là bắt buộc.")]
    [MaxLength(20, ErrorMessage = "Mã chương trình đào tạo tối đa 20 ký tự.")]
    string Ma,

    [Required(ErrorMessage = "Tên chương trình đào tạo là bắt buộc.")]
    [MaxLength(200, ErrorMessage = "Tên chương trình đào tạo tối đa 200 ký tự.")]
    string Ten,

    [Required(ErrorMessage = "NganhId là bắt buộc.")]
    int NganhId,

    [Required(ErrorMessage = "Khóa tuyển là bắt buộc (ví dụ: 2025).")]
    [Range(2000, 2100, ErrorMessage = "Khóa tuyển hợp lệ từ 2000 đến 2100.")]
    int KhoaTuyen,

    [Range(0, 500, ErrorMessage = "Tổng số tín chỉ từ 0 đến 500.")]
    int TongSoTinChi = 0
);

public class ChuongTrinhDaoTaoFilterQuery : PagedQuery
{
    public int? NganhId { get; set; }
    public int? KhoaTuyen { get; set; }
}

// DTO cho Môn học trong CTĐT (EDU-05)
public record MonHocTrongCtdtDto(
    int Id,
    int ChuongTrinhDaoTaoId,
    int MonHocId,
    string? MaMonHoc,
    string? TenMonHoc,
    int? SoTinChi,
    int HocKyGoiY,
    bool BatBuoc,
    string? GhiChu,
    DateTime CreatedAt
);

public record ThemMonHocVaoCtdtRequest(
    [Required(ErrorMessage = "MonHocId là bắt buộc.")]
    int MonHocId,

    [Required(ErrorMessage = "Học kỳ gợi ý là bắt buộc.")]
    [Range(1, 20, ErrorMessage = "Học kỳ gợi ý từ 1 đến 20.")]
    int HocKyGoiY,

    bool BatBuoc = true,

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    string? GhiChu = null
);

public record CapNhatMonHocTrongCtdtRequest(
    [Required(ErrorMessage = "Học kỳ gợi ý là bắt buộc.")]
    [Range(1, 20, ErrorMessage = "Học kỳ gợi ý từ 1 đến 20.")]
    int HocKyGoiY,

    bool BatBuoc = true,

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
    string? GhiChu = null
);

public record ChuongTrinhDaoTaoDetailDto(
    int Id,
    string Ma,
    string Ten,
    int NganhId,
    string? TenNganh,
    int KhoaTuyen,
    int TongSoTinChi,
    List<MonHocTrongCtdtDto> DanhSachMonHoc,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
