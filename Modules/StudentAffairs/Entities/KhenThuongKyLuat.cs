using StudentManagement.Common;

namespace StudentManagement.Modules.StudentAffairs.Entities;

/// <summary>Quyết định khen thưởng hoặc kỷ luật.</summary>
public class KhenThuongKyLuat : BaseEntity
{
    public int SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id)
    public LoaiKhenThuongKyLuat Loai { get; set; } = LoaiKhenThuongKyLuat.KhenThuong;
    public string? SoQuyetDinh { get; set; }
    public DateOnly NgayQuyetDinh { get; set; }
    public string NoiDung { get; set; } = string.Empty;
}
