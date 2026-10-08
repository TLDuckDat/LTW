using StudentManagement.Common;

namespace StudentManagement.Modules.Courses.Entities;

/// <summary>Môn học.</summary>
public class MonHoc : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
}
