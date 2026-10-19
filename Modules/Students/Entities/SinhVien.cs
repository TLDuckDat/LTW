using StudentManagement.Common;

namespace StudentManagement.Modules.Students.Entities;

/// <summary>Hồ sơ sinh viên.</summary>
public class SinhVien : BaseEntity
{
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateOnly NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? CCCD { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public int NganhId { get; set; } // -> Academic.Nganh (khác module: chỉ giữ Id)
    public int? ChuyenNganhId { get; set; } // -> Academic.ChuyenNganh (khác module: chỉ giữ Id)
    public int? LopHanhChinhId { get; set; } // -> LopHanhChinh
    public TrangThaiSinhVien TrangThai { get; set; } = TrangThaiSinhVien.DangHoc;
}

