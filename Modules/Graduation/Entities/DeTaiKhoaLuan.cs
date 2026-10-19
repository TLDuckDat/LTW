using StudentManagement.Common;

namespace StudentManagement.Modules.Graduation.Entities;

/// <summary>Đề tài khóa luận tốt nghiệp.</summary>
public class DeTaiKhoaLuan : BaseEntity
{
    public string TenDeTai { get; set; } = string.Empty;
    public int? SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id)
    public int? GiangVienHuongDanId { get; set; } // -> Students.GiangVien (khác module: chỉ giữ Id)
    public int? HoiDongBaoVeId { get; set; } // -> HoiDongBaoVe
    public decimal? Diem { get; set; }
    public string? NhanXetBaoVe { get; set; }
    public DateTime? NgayGhiNhanKetQua { get; set; }
}
