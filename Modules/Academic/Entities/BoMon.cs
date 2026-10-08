using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Bộ môn trực thuộc khoa.</summary>
public class BoMon : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public int KhoaId { get; set; } // -> Khoa
}
