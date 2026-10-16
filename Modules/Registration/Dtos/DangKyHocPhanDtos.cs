using System.ComponentModel.DataAnnotations;

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
