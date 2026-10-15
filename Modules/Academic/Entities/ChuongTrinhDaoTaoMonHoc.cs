using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Môn học trong chương trình đào tạo.</summary>
public class ChuongTrinhDaoTaoMonHoc : BaseEntity
{
    public int ChuongTrinhDaoTaoId { get; set; } // -> ChuongTrinhDaoTao
    public int MonHocId { get; set; } // -> Courses.MonHoc (khác module: chỉ giữ Id)
    public int HocKyGoiY { get; set; } // Học kỳ dự kiến (1, 2, 3...)
    public bool BatBuoc { get; set; } = true; // true: Bắt buộc, false: Tự chọn
    public string? GhiChu { get; set; }
}
