using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Kiểm tra xung đột thời khóa biểu về phòng học, giảng viên (CRS-06).</summary>
public interface IXungDotLichService
{
    /// <summary>
    /// Tìm các buổi học đã có (cùng học kỳ, lớp chưa hủy) giao nhau về thời gian với buổi học dự kiến
    /// và trùng phòng hoặc trùng giảng viên. <paramref name="boQuaLichHocId"/> là buổi đang sửa.
    /// </summary>
    Task<IReadOnlyList<XungDotDto>> TimXungDotAsync(
        LopHocPhan lop, int? giangVienId, int? phongHocId, int thu, int tietBatDau, int soTiet, int? boQuaLichHocId = null);

    /// <summary>Kiểm tra giảng viên dự kiến phân công có bị trùng giờ với lớp khác không, theo các buổi đã xếp của lớp.</summary>
    Task<IReadOnlyList<XungDotDto>> TimXungDotGiangVienAsync(LopHocPhan lop, int giangVienId);
}
