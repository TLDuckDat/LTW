using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;

namespace StudentManagement.Modules.Students.Services;

/// <summary>Nghiệp vụ quản lý giảng viên / cố vấn học tập (STU-05).</summary>
public interface IGiangVienService
{
    Task<PagedResult<GiangVienDto>> GetPagedAsync(GiangVienFilterQuery query);
    Task<GiangVienDto?> GetByIdAsync(int id);
    Task<GiangVienDto> CreateAsync(GiangVienRequest request);
    Task<GiangVienDto?> UpdateAsync(int id, GiangVienRequest request);
    Task<bool> DeleteAsync(int id);
}
