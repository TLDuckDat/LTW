using StudentManagement.Common;

namespace StudentManagement.Modules.Graduation.Entities;

/// <summary>Kết quả xét tốt nghiệp của sinh viên.</summary>
public class XetTotNghiep : BaseEntity
{
    public int SinhVienId { get; set; } // -> Students.SinhVien (khác module: chỉ giữ Id)
    public DateOnly NgayXet { get; set; }
    public int? TongTinChiTichLuy { get; set; }
    public decimal? DiemTrungBinhTichLuy { get; set; }
    public string? XepLoai { get; set; }
    public string? LyDo { get; set; }
    public KetQuaXetTotNghiep KetQua { get; set; } = KetQuaXetTotNghiep.ChoXet;
}
