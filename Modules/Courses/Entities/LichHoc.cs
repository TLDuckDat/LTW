using StudentManagement.Common;

namespace StudentManagement.Modules.Courses.Entities;

/// <summary>Buổi học trong tuần của lớp học phần.</summary>
public class LichHoc : BaseEntity
{
    public int LopHocPhanId { get; set; } // -> LopHocPhan
    public int Thu { get; set; }
    public int TietBatDau { get; set; }
    public int SoTiet { get; set; }
    public int? PhongHocId { get; set; } // -> PhongHoc
}
