using StudentManagement.Common;

namespace StudentManagement.Modules.Registration.Entities;

/// <summary>Đợt đăng ký học phần.</summary>
public class DotDangKy : BaseEntity
{
    public string Ten { get; set; } = string.Empty;
    public int HocKyId { get; set; } // -> Academic.HocKy (khác module: chỉ giữ Id)
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public TrangThaiDotDangKy TrangThai { get; set; } = TrangThaiDotDangKy.ChuaMo;
}
