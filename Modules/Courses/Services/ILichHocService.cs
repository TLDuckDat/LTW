using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Xếp thời khóa biểu cho lớp học phần và kiểm tra xung đột (CRS-05, CRS-06).</summary>
public interface ILichHocService
{
    Task<IReadOnlyList<LichHocDto>?> GetByLopAsync(int lopHocPhanId);
    Task<LichHocDto?> AddAsync(int lopHocPhanId, LichHocRequest request);
    Task<LichHocDto?> UpdateAsync(int lopHocPhanId, int lichHocId, LichHocRequest request);
    Task<KiemTraXungDotDto?> KiemTraXungDotAsync(int lopHocPhanId, LichHocRequest request, int? lichHocId);
    Task<bool> DeleteAsync(int lopHocPhanId, int lichHocId);
}
