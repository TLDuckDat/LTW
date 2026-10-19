using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

public interface INamHocService
{
    Task<PagedResult<NamHocDto>> GetPagedAsync(PagedQuery query);
    Task<NamHocDto> GetByIdAsync(int id);
    Task<NamHocDto> CreateAsync(NamHocRequest request);
    Task<NamHocDto> UpdateAsync(int id, NamHocRequest request);
    Task DeleteAsync(int id);
}
