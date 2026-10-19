using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

public interface IHocKyService
{
    Task<PagedResult<HocKyDto>> GetPagedAsync(HocKyFilterQuery query);
    Task<HocKyDto?> GetByIdAsync(int id);
    Task<HocKyDto> CreateAsync(HocKyRequest request);
    Task<HocKyDto?> UpdateAsync(int id, HocKyRequest request);
    Task<bool> DeleteAsync(int id);
}
