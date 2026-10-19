using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Graduation.Dtos;

public record NhapKetQuaBaoVeRequest(
    [Required(ErrorMessage = "Điểm bảo vệ là bắt buộc.")]
    [Range(typeof(decimal), "0", "10", ErrorMessage = "Điểm bảo vệ phải từ 0 đến 10.")]
    decimal? Diem,
    [MaxLength(1000, ErrorMessage = "Nhận xét không được quá 1000 ký tự.")]
    string? NhanXetBaoVe);

public class KetQuaBaoVeQuery : PagedQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã sinh viên phải lớn hơn 0.")]
    public int? SinhVienId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Mã hội đồng phải lớn hơn 0.")]
    public int? HoiDongBaoVeId { get; set; }
    public bool? DaCoKetQua { get; set; }
}

public record KetQuaBaoVeDto(int DeTaiId, string TenDeTai,
    int? SinhVienId, string? MaSinhVien, string? HoTenSinhVien,
    int? HoiDongBaoVeId, string? TenHoiDong,
    decimal? Diem, string? NhanXetBaoVe, DateTime? NgayGhiNhanKetQua,
    bool DaCoKetQua);
