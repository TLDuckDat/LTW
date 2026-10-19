using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

public interface IQuyCheHocVuService
{
    Task<PagedResult<QuyCheHocVuDto>> GetPagedAsync(QuyCheHocVuFilterQuery query);
    Task<QuyCheHocVuDto> GetByIdAsync(int id);
    Task<QuyCheHocVuDto> GetByMaAsync(string ma);
    Task<QuyCheHocVuDto> CreateAsync(QuyCheHocVuRequest request);
    Task<QuyCheHocVuDto> UpdateAsync(int id, QuyCheHocVuRequest request);
    Task DeleteAsync(int id);
}
