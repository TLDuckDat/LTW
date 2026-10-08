using StudentManagement.Common;

namespace StudentManagement.Modules.Graduation.Entities;

/// <summary>Hội đồng bảo vệ khóa luận.</summary>
public class HoiDongBaoVe : BaseEntity
{
    public string Ten { get; set; } = string.Empty;
    public DateTime NgayBaoVe { get; set; }
    public int ChuTichId { get; set; } // -> Students.GiangVien (khác module: chỉ giữ Id)
    public int? ThuKyId { get; set; } // -> Students.GiangVien (khác module: chỉ giữ Id)
}
