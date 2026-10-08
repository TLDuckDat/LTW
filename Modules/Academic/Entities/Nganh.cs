using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Ngành đào tạo.</summary>
public class Nganh : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public int KhoaId { get; set; } // -> Khoa
}
