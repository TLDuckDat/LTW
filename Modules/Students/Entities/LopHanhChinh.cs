using StudentManagement.Common;

namespace StudentManagement.Modules.Students.Entities;

/// <summary>Lớp hành chính (lớp sinh hoạt).</summary>
public class LopHanhChinh : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public int NganhId { get; set; } // -> Academic.Nganh (khác module: chỉ giữ Id)
    public int KhoaTuyen { get; set; }
    public int? CoVanId { get; set; } // -> GiangVien
}
