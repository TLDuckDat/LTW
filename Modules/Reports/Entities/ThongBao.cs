using StudentManagement.Common;

namespace StudentManagement.Modules.Reports.Entities;

/// <summary>Thông báo gửi tới người dùng.</summary>
public class ThongBao : BaseEntity
{
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public int NguoiTaoId { get; set; } // -> Auth.TaiKhoan (khác module: chỉ giữ Id)
    public DateTime NgayDang { get; set; }
}
