using StudentManagement.Common;

namespace StudentManagement.Modules.Reports.Dtos;

/// <summary>Một dòng thống kê số lượng theo nhóm. Id là khóa của nhóm (khoa, ngành...) nếu có.</summary>
public record ThongKeItemDto(int? Id, string Ma, string Ten, int SoLuong);

// RPT-01
/// <summary>Số sinh viên theo khoa, ngành và trạng thái học tập (kể cả nhóm chưa có sinh viên).</summary>
public record ThongKeSinhVienDto(
    int TongSoSinhVien,
    IReadOnlyList<ThongKeItemDto> TheoKhoa,
    IReadOnlyList<ThongKeItemDto> TheoNganh,
    IReadOnlyList<ThongKeItemDto> TheoTrangThai);

// RPT-02
/// <summary>Kết quả một môn học trong học kỳ; chỉ điểm đã khóa mới được tính là có kết quả.</summary>
public record KetQuaMonHocDto(
    int MonHocId,
    string MaMonHoc,
    string TenMonHoc,
    int SoTinChi,
    int SoLuotDangKy,
    int SoLuotCoKetQua,
    int SoLuotDat,
    decimal? TyLeDat,
    decimal? DiemTrungBinh);

/// <summary>
/// Kết quả học tập của học kỳ. Xếp loại chỉ tính trên GPA chính thức (mọi học phần trong kỳ đã khóa điểm);
/// sinh viên chưa có GPA hoặc GPA còn tạm tính được đếm vào SoSinhVienChuaDuKetQua.
/// </summary>
public record ThongKeKetQuaHocTapDto(
    int HocKyId,
    string TenHocKy,
    string TenNamHoc,
    int SoSinhVien,
    int SoSinhVienCoGpa,
    int SoSinhVienChuaDuKetQua,
    decimal? GpaTrungBinh,
    IReadOnlyList<ThongKeItemDto> TheoXepLoai,
    IReadOnlyList<ThongKeItemDto> TheoDiemChu,
    IReadOnlyList<KetQuaMonHocDto> TheoMonHoc);

// RPT-03
public record CongNoSinhVienDto(
    int HocPhiId, int SinhVienId, string MSSV, string HoTen, decimal SoTien, decimal SoTienDaNop, decimal SoConNo);

/// <summary>
/// Công nợ học phí của học kỳ. DaThu chỉ tính phần không vượt số phải nộp; phần nộp thừa (cần hoàn)
/// nằm ở CanHoan. SoSinhVienChuaTinhHocPhi: sinh viên đã đăng ký học phần nhưng chưa có bản ghi
/// học phí (cần chạy tính học phí trước khi đối soát).
/// </summary>
public record BaoCaoCongNoDto(
    int HocKyId,
    string TenHocKy,
    int SoKhoanHocPhi,
    int SoSinhVienConNo,
    int SoSinhVienChuaTinhHocPhi,
    decimal TongPhaiThu,
    decimal DaThu,
    decimal ConNo,
    decimal CanHoan,
    PagedResult<CongNoSinhVienDto> DanhSachConNo);
