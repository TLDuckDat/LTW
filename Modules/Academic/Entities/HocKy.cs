using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Học kỳ thuộc năm học.</summary>
public class HocKy : BaseEntity
{
    public string Ten { get; set; } = string.Empty;
    public int SoThuTu { get; set; }
    public int NamHocId { get; set; } // -> NamHoc
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
}
