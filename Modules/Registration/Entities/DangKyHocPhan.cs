using StudentManagement.Common;

namespace StudentManagement.Modules.Registration.Entities;

/// <summary>Sinh viên đăng ký vào lớp học phần.</summary>
public class DangKyHocPhan : BaseEntity
{
    public int SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id)
    public int LopHocPhanId { get; set; } // -> Courses.LopHocPhan (khác module: chỉ giữ Id)
    public LoaiDangKy LoaiDangKy { get; set; } = LoaiDangKy.HocMoi;
    public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.DaDangKy;
}
