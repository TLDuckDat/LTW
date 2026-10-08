using StudentManagement.Common;

namespace StudentManagement.Modules.Courses.Entities;

/// <summary>Lớp học phần mở trong học kỳ.</summary>
public class LopHocPhan : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public int HocKyId { get; set; } // -> Academic.HocKy (khác module: chỉ giữ Id)
    public int MonHocId { get; set; } // -> MonHoc
    public int? GiangVienId { get; set; } // -> Students.GiangVien (khác module: chỉ giữ Id)
    public int SiSoToiDa { get; set; }
    public TrangThaiLopHocPhan TrangThai { get; set; } = TrangThaiLopHocPhan.MoDangKy;
}
