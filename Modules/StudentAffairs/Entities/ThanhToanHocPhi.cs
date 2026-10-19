using StudentManagement.Common;

namespace StudentManagement.Modules.StudentAffairs.Entities;

/// <summary>Một lần nộp học phí (cho phép nộp nhiều lần).</summary>
public class ThanhToanHocPhi : BaseEntity
{
    public int HocPhiId { get; set; } // -> HocPhi
    public decimal SoTien { get; set; }
    public DateTime NgayThanhToan { get; set; }
    public string? HinhThuc { get; set; }
    public string? GhiChu { get; set; }
    public int? NguoiThuId { get; set; } // -> Auth.TaiKhoan (khác module: chỉ giữ Id)
}
