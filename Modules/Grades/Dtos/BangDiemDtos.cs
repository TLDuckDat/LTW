namespace StudentManagement.Modules.Grades.Dtos;

/// <summary>
/// Một lượt học trong bảng điểm sinh viên. DiemHe4 chỉ có khi điểm đã được khóa.
/// DiemHocPhanId dùng để tạo yêu cầu phúc khảo; null khi lượt học chưa được nhập điểm.
/// </summary>
public record BangDiemMonHocDto(
    int DangKyHocPhanId,
    int? DiemHocPhanId,
    int MonHocId,
    string MaMonHoc,
    string TenMonHoc,
    int SoTinChi,
    string LoaiDangKy,
    decimal? DiemTongKet,
    string? DiemChu,
    decimal? DiemHe4,
    bool DaKhoa);

/// <summary>Kết quả của một học kỳ; GPA có thể tạm tính nếu còn học phần chờ điểm khóa.</summary>
public record BangDiemHocKyDto(
    int HocKyId,
    string TenHocKy,
    string TenNamHoc,
    decimal? Gpa,
    decimal? Cpa,
    int TongTinChiTinhGpa,
    int SoHocPhanChuaCoKetQuaKhoa,
    bool GpaTamTinh,
    IReadOnlyList<BangDiemMonHocDto> DanhSachMonHoc);

/// <summary>Bảng điểm và kết quả tích lũy hiện tại của sinh viên.</summary>
public record BangDiemSinhVienDto(
    int SinhVienId,
    string MSSV,
    string HoTen,
    decimal? Cpa,
    int TongTinChiTinhCpa,
    int TongTinChiTichLuyDat,
    IReadOnlyList<BangDiemHocKyDto> DanhSachHocKy);
