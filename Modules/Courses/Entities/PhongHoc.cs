using StudentManagement.Common;

namespace StudentManagement.Modules.Courses.Entities;

/// <summary>Phòng học.</summary>
public class PhongHoc : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public int SucChua { get; set; }
}
