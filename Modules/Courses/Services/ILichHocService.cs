using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Xếp thời khóa biểu cho lớp học phần (CRS-05).</summary>
public interface ILichHocService
{
    Task<IReadOnlyList<LichHocDto>?> GetByLopAsync(int lopHocPhanId);
    Task<LichHocDto?> AddAsync(int lopHocPhanId, LichHocRequest request);
    Task<LichHocDto?> UpdateAsync(int lopHocPhanId, int lichHocId, LichHocRequest request);
    Task<bool> DeleteAsync(int lopHocPhanId, int lichHocId);
}
