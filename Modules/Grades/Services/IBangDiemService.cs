using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>GRADE-06: bảng điểm sinh viên, GPA từng học kỳ và CPA tích lũy.</summary>
public interface IBangDiemService
{
    Task<BangDiemSinhVienDto> GetBangDiemAsync(int sinhVienId);

    /// <summary>Bảng điểm của nhiều sinh viên chỉ với hai truy vấn; sinh viên không tồn tại sẽ không có trong kết quả.</summary>
    Task<Dictionary<int, BangDiemSinhVienDto>> GetBangDiemNhieuSinhVienAsync(int[] sinhVienIds);
}
