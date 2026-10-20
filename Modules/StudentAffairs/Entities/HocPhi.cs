using StudentManagement.Common;

namespace StudentManagement.Modules.StudentAffairs.Entities;

/// <summary>
/// Học phí phải nộp theo học kỳ. SoTienDaNop là tổng các lần nộp (bảng ThanhToanHocPhi);
/// DaThanhToan = đã nộp đủ (SoTienDaNop >= SoTien), luôn được service cập nhật cùng hai cột trên.
/// </summary>
public class HocPhi : BaseEntity
{
    public int SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id)
    public int HocKyId { get; set; } // -> Academic.HocKy (khác module: chỉ giữ Id)
    public decimal SoTien { get; set; }
    public decimal SoTienDaNop { get; set; }
    public bool DaThanhToan { get; set; }
    /// <summary>Do SQL Server tự tăng; tính lại học phí đúng lúc có lần nộp mới thì báo 409 thay vì ghi DaThanhToan sai.</summary>
    public byte[] RowVersion { get; set; } = [];
}
