using StudentManagement.Common;

namespace StudentManagement.Modules.Graduation.Entities;

/// <summary>Thực tập của sinh viên.</summary>
public class ThucTap : BaseEntity
{
    public int SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id)
    public int HocKyId { get; set; } // -> Academic.HocKy (khác module: chỉ giữ Id)
    public int CoSoThucTapId { get; set; }
    // Chỉ giữ dữ liệu lịch sử; đăng ký mới dùng CoSoThucTapId.
    public string? DonViThucTap { get; set; }
    public int? GiangVienHuongDanId { get; set; } // -> Students.GiangVien (khác module: chỉ giữ Id)
    public decimal? Diem { get; set; }
    public string? NhanXet { get; set; }
    public DateTime? NgayGhiNhanKetQua { get; set; }
    public TrangThaiThucTap TrangThai { get; set; } = TrangThaiThucTap.DangKy;
}
