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
    /// <summary>Do SQL Server tự tăng; hai người cùng xử lý một đơn thì người lưu sau nhận 409.</summary>
    public byte[] RowVersion { get; set; } = [];
}
