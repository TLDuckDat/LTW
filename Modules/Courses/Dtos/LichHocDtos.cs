using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Courses.Dtos;

/// <summary>Buổi học trong tuần của lớp học phần (Thu: 2-7 là thứ Hai-thứ Bảy, 8 là Chủ nhật).</summary>
public record LichHocDto(int Id, int LopHocPhanId, int Thu, int TietBatDau, int SoTiet, int? PhongHocId, string? MaPhong);

/// <summary>Dữ liệu xếp / sửa một buổi học.</summary>
public record LichHocRequest(
    [Range(2, 8)] int Thu,
    [Range(1, 12)] int TietBatDau,
    [Range(1, 12)] int SoTiet,
    [Range(1, int.MaxValue)] int PhongHocId);
