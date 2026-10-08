using StudentManagement.Common;

namespace StudentManagement.Modules.Courses.Entities;

/// <summary>Quan hệ môn tiên quyết: phải học TienQuyetId trước MonHocId.</summary>
public class MonHocTienQuyet : BaseEntity
{
    public int MonHocId { get; set; } // -> MonHoc
    public int TienQuyetId { get; set; } // -> MonHoc
}
