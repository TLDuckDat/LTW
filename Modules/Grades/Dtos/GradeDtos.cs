using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentManagement.Modules.Grades.Dtos;

/// <summary>Sinh viên trong danh sách lớp học phần (chỉ các lượt đăng ký còn hiệu lực).</summary>
public record SinhVienTrongLopDto(int DangKyHocPhanId, string MSSV, string HoTen, string LoaiDangKy);

/// <summary>Lượt đăng ký học phần trả về sau khi thêm hoặc chuyển lớp.</summary>
public record DangKyHocPhanDto(int DangKyHocPhanId, int SinhVienId, int LopHocPhanId, string LoaiDangKy, string TrangThai);

/// <summary>Thêm sinh viên vào lớp học phần.</summary>
public record ThemSinhVienRequest([Range(1, int.MaxValue)] int SinhVienId);

/// <summary>Chuyển lượt đăng ký sang lớp học phần khác cùng môn.</summary>
public record ChuyenLopRequest([Range(1, int.MaxValue)] int LopHocPhanMoiId);

/// <summary>Điểm của một lượt đăng ký. Điểm null nghĩa là chưa có điểm thành phần đó.</summary>
// Range dùng 0d/10d (kiểu double): nếu dùng số nguyên, decimal 10.5 bị làm tròn thành 10 và lọt qua kiểm tra.
// JsonRequired: bắt buộc có đủ 4 trường trong JSON. Điểm null vẫn hợp lệ, nhưng thiếu trường thì trả 400.
public record DiemSinhVienRequest(
    [property: JsonRequired] [Range(1, int.MaxValue)] int DangKyHocPhanId,
    [property: JsonRequired] [Range(0d, 10d)] decimal? DiemQuaTrinh,
    [property: JsonRequired] [Range(0d, 10d)] decimal? DiemGiuaKy,
    [property: JsonRequired] [Range(0d, 10d)] decimal? DiemCuoiKy);

/// <summary>Danh sách điểm nhập hoặc cập nhật cho nhiều lượt đăng ký trong một lớp học phần.</summary>
public record NhapDiemRequest([Required, MinLength(1)] List<DiemSinhVienRequest> DanhSachDiem);

/// <summary>Kết quả nhập điểm: số lượt đã gửi lên và số lượt thêm mới, cập nhật, không đổi.</summary>
public record NhapDiemResultDto(int LopHocPhanId, int TongSoGuiLen, int SoThemMoi, int SoCapNhat, int SoKhongThayDoi);

/// <summary>Kết quả tính điểm của các lượt đăng ký còn hiệu lực trong lớp học phần.</summary>
public record TinhDiemResultDto(
    int LopHocPhanId,
    int TongSoBanGhiDiem,
    int SoDaTinh,
    int SoThieuDiem,
    int SoDaKhoa,
    int SoKhongThayDoi);

/// <summary>Kết quả duyệt và khóa bảng điểm của lớp học phần.</summary>
public record KhoaDiemResultDto(int LopHocPhanId, int TongSoSinhVien, int SoMoiKhoa, int SoDaKhoaTruocDo);

/// <summary>Một lượt học trong bảng điểm sinh viên.</summary>
public record BangDiemMonHocDto(
    int DangKyHocPhanId,
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

/// <summary>Đánh giá học vụ từ dữ liệu hiện tại; chưa phải quyết định của Phòng đào tạo.</summary>
public record CanhBaoHocVuDto(
    int SinhVienId,
    string MSSV,
    string HoTen,
    int HocKyId,
    string TenHocKy,
    string TenNamHoc,
    decimal? Gpa,
    decimal? Cpa,
    int TongTinChiDangKy,
    int SoHocPhanKhongDat,
    int SoTinChiKhongDat,
    string TrangThaiCanhBao,
    IReadOnlyList<string> DanhSachLyDo,
    string GhiChu);

/// <summary>Tạo yêu cầu phúc khảo điểm cuối kỳ.</summary>
public record PhucKhaoRequest(int DiemHocPhanId, string? LyDo);

/// <summary>Quyết định phúc khảo; chỉ cung cấp điểm mới khi chấp nhận.</summary>
public record XuLyPhucKhaoRequest(bool? ChapNhan, decimal? DiemMoi);

/// <summary>Thông tin yêu cầu phúc khảo và sinh viên liên quan.</summary>
public record PhucKhaoDto(
    int Id,
    int DiemHocPhanId,
    int SinhVienId,
    string MSSV,
    string HoTen,
    string LyDo,
    decimal? DiemCu,
    decimal? DiemMoi,
    string TrangThai,
    DateTime NgayTao);
