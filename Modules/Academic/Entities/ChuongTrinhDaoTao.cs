using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Chương trình đào tạo của ngành theo khóa tuyển.</summary>
public class ChuongTrinhDaoTao : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public int NganhId { get; set; } // -> Nganh
    public int KhoaTuyen { get; set; }
    public int TongSoTinChi { get; set; }
}
