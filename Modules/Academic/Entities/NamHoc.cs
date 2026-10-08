using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Năm học, ví dụ 2025-2026.</summary>
public class NamHoc : BaseEntity
{
    public string Ten { get; set; } = string.Empty;
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
}
