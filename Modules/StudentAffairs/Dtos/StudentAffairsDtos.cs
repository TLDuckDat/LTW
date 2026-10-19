using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Entities;

namespace StudentManagement.Modules.StudentAffairs.Dtos;

/// <summary>Bộ lọc chung: theo sinh viên, học kỳ; Search tìm theo MSSV hoặc họ tên.</summary>
public class SinhVienHocKyQuery : PagedQuery
{
    public int? SinhVienId { get; set; }
    public int? HocKyId { get; set; }
}

// ===== SA-01 Điểm rèn luyện =====
public record DiemRenLuyenDto(
    int Id, int SinhVienId, string MSSV, string HoTen, int HocKyId, string TenHocKy,
    int Diem, string? XepLoai, DateTime CreatedAt, DateTime? UpdatedAt);

public record DiemRenLuyenRequest(
    [Range(1, int.MaxValue)] int SinhVienId,
    [Range(1, int.MaxValue)] int HocKyId,
    [Range(0, 100, ErrorMessage = "Điểm rèn luyện phải từ 0 đến 100.")] int Diem);

public record CapNhatDiemRenLuyenRequest(
    [Range(0, 100, ErrorMessage = "Điểm rèn luyện phải từ 0 đến 100.")] int Diem);

// ===== SA-02, SA-03 Học phí =====
public class HocPhiQuery : SinhVienHocKyQuery
{
    /// <summary>true: chỉ khoản còn nợ; false: chỉ khoản đã nộp đủ.</summary>
    public bool? ConNo { get; set; }
}

/// <summary>SoConNo: số còn phải nộp; SoTienThua: số đã nộp vượt (cần hoàn) khi học phí giảm sau khi nộp.</summary>
public record HocPhiDto(
    int Id, int SinhVienId, string MSSV, string HoTen, int HocKyId, string TenHocKy,
    decimal SoTien, decimal SoTienDaNop, decimal SoConNo, decimal SoTienThua, bool DaThanhToan,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record TinhHocPhiResultDto(
    int HocKyId, string TenHocKy, decimal DonGiaTinChi,
    int SoSinhVien, int SoTaoMoi, int SoCapNhat, int SoKhongThayDoi,
    int SoCanHoanTien, decimal TongPhaiThu);

/// <summary>SoTien bỏ trống: nộp toàn bộ số còn nợ.</summary>
public record ThanhToanRequest(
    [Range(typeof(decimal), "0.01", "1000000000000")] decimal? SoTien = null,
    [MaxLength(50)] string? HinhThuc = null,
    [MaxLength(500)] string? GhiChu = null);

public record ThanhToanHocPhiDto(
    int Id, int HocPhiId, decimal SoTien, DateTime NgayThanhToan,
    string? HinhThuc, string? GhiChu, int? NguoiThuId, string? TenNguoiThu);

public record ThanhToanResultDto(HocPhiDto HocPhi, ThanhToanHocPhiDto ThanhToan);

// ===== SA-04 Miễn giảm =====
public record MienGiamDto(
    int Id, int SinhVienId, string MSSV, string HoTen, int HocKyId, string TenHocKy,
    decimal TyLe, string? LyDo, DateTime CreatedAt, DateTime? UpdatedAt);

public record MienGiamRequest(
    [Range(1, int.MaxValue)] int SinhVienId,
    [Range(1, int.MaxValue)] int HocKyId,
    [Range(typeof(decimal), "0", "100", ErrorMessage = "Tỷ lệ miễn giảm phải từ 0 đến 100.")] decimal TyLe,
    [MaxLength(500)] string? LyDo);

public record CapNhatMienGiamRequest(
    [Range(typeof(decimal), "0", "100", ErrorMessage = "Tỷ lệ miễn giảm phải từ 0 đến 100.")] decimal TyLe,
    [MaxLength(500)] string? LyDo);

// ===== SA-05 Học bổng =====
public record HocBongDto(
    int Id, int SinhVienId, string MSSV, string HoTen, int HocKyId, string TenHocKy,
    string Ten, decimal SoTien, DateTime CreatedAt);

public record XetHocBongKhongDatDto(int SinhVienId, string MSSV, string HoTen, IReadOnlyList<string> LyDo);

/// <summary>Xét trên các sinh viên có điểm rèn luyện trong học kỳ; SoDaCoTruocDo là người đã được cấp từ lần xét trước.</summary>
public record XetHocBongResultDto(
    int HocKyId, string TenHocKy, string TenHocBong,
    int DiemRenLuyenToiThieu, decimal GpaToiThieu, decimal SoTien,
    int SoSinhVienXet, int SoMoiCap, int SoDaCoTruocDo,
    IReadOnlyList<HocBongDto> DanhSachMoiCap,
    IReadOnlyList<XetHocBongKhongDatDto> DanhSachKhongDat);

// ===== SA-06 Khen thưởng / kỷ luật =====
public class KhenThuongKyLuatQuery : PagedQuery
{
    public int? SinhVienId { get; set; }
    public LoaiKhenThuongKyLuat? Loai { get; set; }
}

public record KhenThuongKyLuatDto(
    int Id, int SinhVienId, string MSSV, string HoTen, string Loai,
    string? SoQuyetDinh, DateOnly NgayQuyetDinh, string NoiDung, DateTime CreatedAt, DateTime? UpdatedAt);

public record KhenThuongKyLuatRequest(
    [Range(1, int.MaxValue)] int SinhVienId,
    [Required, EnumDataType(typeof(LoaiKhenThuongKyLuat))] LoaiKhenThuongKyLuat? Loai,
    [MaxLength(50)] string? SoQuyetDinh,
    [Required] DateOnly? NgayQuyetDinh,
    [Required, MaxLength(1000)] string NoiDung);
