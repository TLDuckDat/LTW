using StudentManagement.Common;

namespace StudentManagement.Modules.Graduation.Entities;

public class CoSoThucTap : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? NguoiLienHe { get; set; }
    public bool DangTiepNhan { get; set; } = true;
}
