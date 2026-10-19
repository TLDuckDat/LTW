using System.ComponentModel.DataAnnotations;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Registration.Dtos;

/// <summary>Dữ liệu đợt đăng ký trả về cho client.</summary>
public record DotDangKyDto(
    int Id,
    string Ten,
    int HocKyId,
    DateTime NgayBatDau,
    DateTime NgayKetThuc,
    string TrangThai,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>Dữ liệu tạo mới đợt đăng ký (trạng thái ban đầu là ChuaMo).</summary>
public record DotDangKyRequest(
    [Required, MaxLength(100)] string Ten,
    [Range(1, int.MaxValue)] int HocKyId,
    DateTime NgayBatDau,
    DateTime NgayKetThuc);

/// <summary>Cập nhật tên và thời gian của đợt đăng ký (không đổi học kỳ).</summary>
public record DotDangKyUpdateRequest(
    [Required, MaxLength(100)] string Ten,
    DateTime NgayBatDau,
    DateTime NgayKetThuc);

/// <summary>Bộ lọc danh sách đợt đăng ký.</summary>
public class DotDangKyQuery : StudentManagement.Common.PagedQuery
{
    public int? HocKyId { get; set; }
    public TrangThaiDotDangKy? TrangThai { get; set; }
}
