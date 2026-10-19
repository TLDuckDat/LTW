using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentManagement.Modules.Grades.Dtos;

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

/// <summary>Điểm của một sinh viên trong bảng điểm lớp; DiemHocPhanId null khi chưa nhập điểm.</summary>
public record DiemTrongLopDto(
    int DangKyHocPhanId,
    int SinhVienId,
    string MSSV,
    string HoTen,
    string LoaiDangKy,
    int? DiemHocPhanId,
    decimal? DiemQuaTrinh,
    decimal? DiemGiuaKy,
    decimal? DiemCuoiKy,
    decimal? DiemTongKet,
    string? DiemChu,
    bool DaKhoa);

/// <summary>Bảng điểm của lớp học phần (các lượt đăng ký còn hiệu lực), sắp theo MSSV.</summary>
public record BangDiemLopDto(int LopHocPhanId, string MaLop, string TrangThaiLop, IReadOnlyList<DiemTrongLopDto> DanhSach);
