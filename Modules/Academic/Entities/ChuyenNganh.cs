using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Chuyên ngành thuộc ngành.</summary>
public class ChuyenNganh : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public int NganhId { get; set; } // -> Nganh
}
