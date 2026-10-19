using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Reports.Dtos;

public record ThongBaoDto(
    int Id,
    string TieuDe,
    string NoiDung,
    DateTime NgayDang,
    int NguoiTaoId,
    string? TenNguoiTao,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>NgayDang (UTC) bỏ trống thì đăng ngay; đặt trong tương lai để hẹn giờ đăng.</summary>
public record ThongBaoRequest(
    [Required, MaxLength(200)] string TieuDe,
    [Required, MaxLength(4000)] string NoiDung,
    DateTime? NgayDang = null);
