using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

public interface IBoMonService
{
    Task<PagedResult<BoMonDto>> GetPagedAsync(BoMonFilterQuery query);
    Task<BoMonDto?> GetByIdAsync(int id);
    Task<BoMonDto> CreateAsync(BoMonRequest request);
    Task<BoMonDto?> UpdateAsync(int id, BoMonRequest request);
    Task<bool> DeleteAsync(int id);
}
