using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Khoa (đơn vị quản lý đào tạo), ví dụ Khoa Công nghệ thông tin.</summary>
public class Khoa : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}
