using System.ComponentModel.DataAnnotations;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Dtos;

/// <summary>Dữ liệu lớp học phần trả về cho client.</summary>
public record LopHocPhanDto(
    int Id,
    string Ma,
    int HocKyId,
    int MonHocId,
    string MaMonHoc,
    string TenMonHoc,
    int? GiangVienId,
    int SiSoToiDa,
    int SiSoHienTai,
    TrangThaiLopHocPhan TrangThai,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>Dữ liệu mở lớp học phần mới trong một học kỳ.</summary>
public record LopHocPhanRequest(
    [Required, MaxLength(30)] string Ma,
    [Range(1, int.MaxValue)] int HocKyId,
    [Range(1, int.MaxValue)] int MonHocId,
    [Range(1, 1000)] int SiSoToiDa);

/// <summary>Cập nhật thông tin lớp học phần (không đổi học kỳ, môn học).</summary>
public record LopHocPhanUpdateRequest(
    [Required, MaxLength(30)] string Ma,
    [Range(1, 1000)] int SiSoToiDa);

/// <summary>Phân công giảng viên cho lớp; GiangVienId = null để bỏ phân công.</summary>
public record PhanCongGiangVienRequest(int? GiangVienId);

/// <summary>Bộ lọc danh sách lớp học phần.</summary>
public class LopHocPhanQuery : StudentManagement.Common.PagedQuery
{
    public int? HocKyId { get; set; }
    public int? MonHocId { get; set; }
    public int? GiangVienId { get; set; }
    public TrangThaiLopHocPhan? TrangThai { get; set; }
}
