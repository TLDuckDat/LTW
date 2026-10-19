using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Quản lý phòng học (CRS-04).</summary>
public interface IPhongHocService
{
    Task<PagedResult<PhongHocDto>> GetPagedAsync(PagedQuery query);
    Task<PhongHocDto> GetByIdAsync(int id);
    Task<PhongHocDto> CreateAsync(PhongHocRequest request);
    Task<PhongHocDto> UpdateAsync(int id, PhongHocRequest request);
    Task DeleteAsync(int id);
}
