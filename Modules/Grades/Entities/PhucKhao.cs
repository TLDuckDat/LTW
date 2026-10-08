using StudentManagement.Common;

namespace StudentManagement.Modules.Grades.Entities;

/// <summary>Đơn phúc khảo điểm.</summary>
public class PhucKhao : BaseEntity
{
    public int DiemHocPhanId { get; set; } // -> DiemHocPhan
    public string LyDo { get; set; } = string.Empty;
    public decimal? DiemCu { get; set; }
    public decimal? DiemMoi { get; set; }
    public TrangThaiPhucKhao TrangThai { get; set; } = TrangThaiPhucKhao.ChoXuLy;
}
