using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Entities;

namespace StudentManagement.Modules.Graduation.Dtos;

public record ThucTapRequest(
    [Range(1, int.MaxValue)] int SinhVienId,
    [Range(1, int.MaxValue)] int HocKyId,
    [Range(1, int.MaxValue)] int CoSoThucTapId,
    [Range(1, int.MaxValue)] int? GiangVienHuongDanId);

public record CapNhatThucTapRequest(
    [Range(1, int.MaxValue)] int CoSoThucTapId,
    [Range(1, int.MaxValue)] int? GiangVienHuongDanId);

public record KetQuaThucTapRequest(
    [Range(typeof(decimal), "0", "10")] decimal Diem,
    [MaxLength(1000)] string? NhanXet);

public record KetQuaThucTapDto(decimal Diem, string? NhanXet, DateTime? NgayGhiNhanKetQua);

public record ThucTapDto(int Id, int SinhVienId, string MaSinhVien, string HoTenSinhVien,
    int HocKyId, string HocKy, int CoSoThucTapId, string TenCoSoThucTap,
    int? GiangVienHuongDanId, string? GiangVienHuongDan,
    string TrangThai, decimal? Diem, string? NhanXet, DateTime? NgayGhiNhanKetQua);

public class ThucTapQuery : PagedQuery
{
    public int? SinhVienId { get; set; }
    public int? HocKyId { get; set; }
    public int? CoSoThucTapId { get; set; }
    public TrangThaiThucTap? TrangThai { get; set; }
}
