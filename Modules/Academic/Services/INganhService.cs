using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

public interface INganhService
{
    Task<PagedResult<NganhDto>> GetPagedAsync(NganhFilterQuery query);
    Task<NganhDto?> GetByIdAsync(int id);
    Task<NganhDto> CreateAsync(NganhRequest request);
    Task<NganhDto?> UpdateAsync(int id, NganhRequest request);
    Task<bool> DeleteAsync(int id);
}
