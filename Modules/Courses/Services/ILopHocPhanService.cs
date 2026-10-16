using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Quản lý lớp học phần: mở lớp theo học kỳ, sĩ số, phân công giảng viên, hủy/gộp lớp.</summary>
public interface ILopHocPhanService
{
    // CRS-02
    Task<PagedResult<LopHocPhanDto>> GetPagedAsync(LopHocPhanQuery query);
    Task<LopHocPhanDto?> GetByIdAsync(int id);
    Task<LopHocPhanDto> CreateAsync(LopHocPhanRequest request);
    Task<LopHocPhanDto?> UpdateAsync(int id, LopHocPhanUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
