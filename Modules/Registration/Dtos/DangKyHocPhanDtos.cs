using System.ComponentModel.DataAnnotations;
using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Registration.Dtos;

/// <summary>Lượt đăng ký học phần của sinh viên.</summary>
public record DangKyDto(
    int Id,
    int SinhVienId,
    int LopHocPhanId,
    string LoaiDangKy,
    string TrangThai,
    DateTime CreatedAt);

/// <summary>
/// Yêu cầu đăng ký học phần. Sinh viên đăng nhập có thể bỏ trống SinhVienId (mặc định là chính mình);
/// phòng đào tạo đăng ký hộ phải truyền SinhVienId.
/// </summary>
public record DangKyRequest(
    int? SinhVienId,
    [Range(1, int.MaxValue)] int LopHocPhanId);

/// <summary>Bộ lọc danh sách lớp học phần mở đăng ký. Bỏ trống HocKyId để lấy học kỳ đang đăng ký.</summary>
public class LopMoQuery : StudentManagement.Common.PagedQuery
{
    public int? SinhVienId { get; set; }
    public int? HocKyId { get; set; }
    public int? MonHocId { get; set; }
}

/// <summary>Lớp học phần đang mở kèm tình trạng đăng ký của sinh viên.</summary>
public record LopMoDto(
    int Id,
    string Ma,
    int HocKyId,
    int MonHocId,
    string MaMonHoc,
    string TenMonHoc,
    int SoTinChi,
    int? GiangVienId,
    string? TenGiangVien,
    int SiSoToiDa,
    int SiSoHienTai,
    int ConCho,
    IReadOnlyList<LichHocDto> LichHoc,
    bool DaDangKy,
    bool DuDieuKien,
    string LoaiDangKy,
    IReadOnlyList<string> LyDoKhongDuDieuKien);

/// <summary>Học phần sinh viên đã đăng ký kèm lịch học.</summary>
public record HocPhanDaDangKyDto(
    int DangKyId,
    int LopHocPhanId,
    string MaLop,
    string MaMonHoc,
    string TenMonHoc,
    int SoTinChi,
    string? TenGiangVien,
    string LoaiDangKy,
    string TrangThaiLop,
    IReadOnlyList<LichHocDto> LichHoc);

/// <summary>Danh sách học phần đã đăng ký của sinh viên trong một học kỳ.</summary>
public record DaDangKyDto(int SinhVienId, int? HocKyId, int TongSoTinChi, IReadOnlyList<HocPhanDaDangKyDto> HocPhan);

/// <summary>Một buổi học trong thời khóa biểu của sinh viên.</summary>
public record BuoiHocDto(
    int Thu,
    int TietBatDau,
    int SoTiet,
    string? MaPhong,
    int LopHocPhanId,
    string MaLop,
    string MaMonHoc,
    string TenMonHoc,
    string? TenGiangVien);

/// <summary>Thời khóa biểu của sinh viên trong một học kỳ, sắp theo thứ rồi tiết.</summary>
public record ThoiKhoaBieuDto(int SinhVienId, int? HocKyId, IReadOnlyList<BuoiHocDto> Buoi);

/// <summary>Học kỳ cần xem; bỏ trống để lấy học kỳ hiện hành.</summary>
public class HocKyQuery
{
    public int? SinhVienId { get; set; }
    public int? HocKyId { get; set; }
}
