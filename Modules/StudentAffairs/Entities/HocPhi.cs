using StudentManagement.Common;

namespace StudentManagement.Modules.StudentAffairs.Entities;

/// <summary>Học phí phải nộp theo học kỳ.</summary>
public class HocPhi : BaseEntity
{
    public int SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id)
    public int HocKyId { get; set; } // -> Academic.HocKy (khác module: chỉ giữ Id)
    public decimal SoTien { get; set; }
    public bool DaThanhToan { get; set; }
}
