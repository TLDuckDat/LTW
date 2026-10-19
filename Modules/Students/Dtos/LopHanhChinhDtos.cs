using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Students.Dtos;

/// <summary>DTO lớp hành chính cơ bản</summary>
public record LopHanhChinhDto(
    int Id,
    string Ma,
    string Ten,
    int NganhId,
    string? TenNganh,
    int KhoaTuyen,
    int? CoVanId,
    string? TenCoVan,
    int SoSinhVien,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>DTO chi tiết lớp hành chính kèm danh sách sinh viên</summary>
public record LopHanhChinhDetailDto(
    int Id,
    string Ma,
    string Ten,
    int NganhId,
    string? TenNganh,
    int KhoaTuyen,
    int? CoVanId,
    string? TenCoVan,
    List<SinhVienDto> SinhViens,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>Request tạo/sửa lớp hành chính (STU-04)</summary>
public record LopHanhChinhRequest
{
    [Required(ErrorMessage = "Mã lớp là bắt buộc.")]
    [MaxLength(20)]
    public string Ma { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tên lớp là bắt buộc.")]
    [MaxLength(100)]
    public string Ten { get; init; } = string.Empty;

    [Required(ErrorMessage = "Ngành đào tạo là bắt buộc.")]
    public int NganhId { get; init; }

    [Range(2000, 2100, ErrorMessage = "Khóa tuyển không hợp lệ.")]
    public int KhoaTuyen { get; init; }

    public int? CoVanId { get; init; }
}

/// <summary>Request phân nhiều sinh viên vào lớp (STU-04)</summary>
public record PhanLopRequest(
    [Required] List<int> SinhVienIds
);

/// <summary>Request phân công cố vấn học tập cho lớp (STU-05)</summary>
public record PhanCoVanRequest(
    int? CoVanId
);
