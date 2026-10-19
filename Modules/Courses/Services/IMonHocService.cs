using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Quản lý môn học và môn học tiên quyết (CRS-01).</summary>
public interface IMonHocService
{
    Task<PagedResult<MonHocDto>> GetPagedAsync(PagedQuery query);
    Task<MonHocDto?> GetByIdAsync(int id);
    Task<MonHocDto> CreateAsync(MonHocRequest request);
    Task<MonHocDto?> UpdateAsync(int id, MonHocRequest request);
    Task<bool> DeleteAsync(int id);

    Task<IReadOnlyList<MonHocDto>?> GetTienQuyetAsync(int id);
    Task<MonHocDto?> AddTienQuyetAsync(int id, TienQuyetRequest request);
    Task<bool> RemoveTienQuyetAsync(int id, int tienQuyetId);
}
