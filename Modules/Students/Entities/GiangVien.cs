using StudentManagement.Common;

namespace StudentManagement.Modules.Students.Entities;

/// <summary>Giảng viên.</summary>
public class GiangVien : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? HocVi { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public int BoMonId { get; set; } // -> Academic.BoMon (khác module: chỉ giữ Id)
}
