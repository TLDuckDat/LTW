using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Grades.Dtos;

/// <summary>Sinh viên trong danh sách lớp học phần (chỉ các lượt đăng ký còn hiệu lực).</summary>
public record SinhVienTrongLopDto(int DangKyHocPhanId, string MSSV, string HoTen, string LoaiDangKy);

/// <summary>Lượt đăng ký học phần trả về sau khi thêm hoặc chuyển lớp.</summary>
public record DangKyHocPhanDto(int DangKyHocPhanId, int SinhVienId, int LopHocPhanId, string LoaiDangKy, string TrangThai);

/// <summary>Thêm sinh viên vào lớp học phần.</summary>
public record ThemSinhVienRequest([Range(1, int.MaxValue)] int SinhVienId);

/// <summary>Chuyển lượt đăng ký sang lớp học phần khác cùng môn.</summary>
public record ChuyenLopRequest([Range(1, int.MaxValue)] int LopHocPhanMoiId);
