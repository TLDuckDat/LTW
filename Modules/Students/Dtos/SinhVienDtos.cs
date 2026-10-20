using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Dtos;

/// <summary>DTO tóm tắt sinh viên cho danh sách</summary>
public record SinhVienDto(
    int Id,
    string MSSV,
    string HoTen,
    DateOnly NgaySinh,
    string? GioiTinh,
    string? Email,
    string? SoDienThoai,
    int NganhId,
    string? TenNganh,
    int? LopHanhChinhId,
    string? TenLopHanhChinh,
    TrangThaiSinhVien TrangThai,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>DTO hồ sơ chi tiết sinh viên (STU-02)</summary>
public record SinhVienDetailDto(
    int Id,
    string MSSV,
    string HoTen,
    DateOnly NgaySinh,
    string? GioiTinh,
    string? CCCD,
    string? Email,
    string? SoDienThoai,
    string? DiaChi,
    int NganhId,
    string? MaNganh,
    string? TenNganh,
    int? KhoaId,
    string? TenKhoa,
    int? ChuyenNganhId,
    string? TenChuyenNganh,
    int? LopHanhChinhId,
    string? MaLop,
    string? TenLop,
    string? TenCoVan,
    TrangThaiSinhVien TrangThai,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>Request tạo mới sinh viên (STU-01)</summary>
public record CreateSinhVienRequest
{
    [Required(ErrorMessage = "MSSV là bắt buộc.")]
    [MaxLength(20, ErrorMessage = "MSSV tối đa 20 ký tự.")]
    public string MSSV { get; init; } = string.Empty;

    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string HoTen { get; init; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
    public DateOnly NgaySinh { get; init; }

    [MaxLength(10)]
    public string? GioiTinh { get; init; }

    [MaxLength(20)]
    public string? CCCD { get; init; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(100)]
    public string? Email { get; init; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [MaxLength(20)]
    public string? SoDienThoai { get; init; }

    [MaxLength(250)]
    public string? DiaChi { get; init; }

    [Required(ErrorMessage = "Ngành đào tạo là bắt buộc.")]
    public int NganhId { get; init; }

    public int? ChuyenNganhId { get; init; }

    public int? LopHanhChinhId { get; init; }
}

/// <summary>Request cập nhật sinh viên (STU-01)</summary>
public record UpdateSinhVienRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string HoTen { get; init; } = string.Empty;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
    public DateOnly NgaySinh { get; init; }

    [MaxLength(10)]
    public string? GioiTinh { get; init; }

    [MaxLength(20)]
    public string? CCCD { get; init; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(100)]
    public string? Email { get; init; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [MaxLength(20)]
    public string? SoDienThoai { get; init; }

    [MaxLength(250)]
    public string? DiaChi { get; init; }

    [Required(ErrorMessage = "Ngành đào tạo là bắt buộc.")]
    public int NganhId { get; init; }

    public int? ChuyenNganhId { get; init; }

    public int? LopHanhChinhId { get; init; }
}

/// <summary>Query lọc danh sách sinh viên đa tiêu chí (STU-03)</summary>
public class SinhVienFilterQuery : PagedQuery
{
    public int? KhoaId { get; set; }
    public int? NganhId { get; set; }
    public int? LopHanhChinhId { get; set; }
    public TrangThaiSinhVien? TrangThai { get; set; }
}

/// <summary>Request đổi trạng thái sinh viên (STU-06)</summary>
public record ChangeStatusRequest(
    [Required] TrangThaiSinhVien TrangThai,
    string? LyDo,
    string? SoQuyetDinh
);

/// <summary>Request bảo lưu (STU-07)</summary>
public record BaoLuuRequest(
    [Required(ErrorMessage = "Lý do bảo lưu là bắt buộc.")]
    [MaxLength(500)]
    string LyDo,
    string? SoQuyetDinh,
    DateOnly? NgayHieuLuc,
    string? GhiChu
);

/// <summary>Request thôi học (STU-07)</summary>
public record ThoiHocRequest(
    [Required(ErrorMessage = "Lý do thôi học là bắt buộc.")]
    [MaxLength(500)]
    string LyDo,
    string? SoQuyetDinh,
    DateOnly? NgayHieuLuc,
    string? GhiChu
);

/// <summary>Request chuyển lớp hành chính (STU-04); null để gỡ sinh viên khỏi lớp</summary>
public record ChuyenLopHanhChinhRequest(
    [Range(1, int.MaxValue)] int? LopHanhChinhId
);

/// <summary>Request quay lại học sau bảo lưu (STU-07)</summary>
public record QuayLaiHocRequest(
    [MaxLength(500)] string? GhiChu
);

/// <summary>DTO lịch sử học tập / thay đổi trạng thái (STU-07)</summary>
public record LichSuHocTapDto(
    int Id,
    int SinhVienId,
    TrangThaiSinhVien TrangThaiCu,
    TrangThaiSinhVien TrangThaiMoi,
    string? SoQuyetDinh,
    DateOnly NgayHieuLuc,
    string LyDo,
    string? GhiChu,
    DateTime CreatedAt
);
