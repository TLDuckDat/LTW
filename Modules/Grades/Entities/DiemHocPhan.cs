using StudentManagement.Common;

namespace StudentManagement.Modules.Grades.Entities;

/// <summary>Điểm của một lượt đăng ký học phần.</summary>
public class DiemHocPhan : BaseEntity
{
    public int DangKyHocPhanId { get; set; } // -> Registration.DangKyHocPhan (khác module: chỉ giữ Id)
    public decimal? DiemQuaTrinh { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public decimal? DiemTongKet { get; set; }
    public string? DiemChu { get; set; }
    public bool DaKhoa { get; set; }
}
