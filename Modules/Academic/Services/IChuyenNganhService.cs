using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

public interface IChuyenNganhService
{
    Task<PagedResult<ChuyenNganhDto>> GetPagedAsync(ChuyenNganhFilterQuery query);
    Task<ChuyenNganhDto?> GetByIdAsync(int id);
    Task<ChuyenNganhDto> CreateAsync(ChuyenNganhRequest request);
    Task<ChuyenNganhDto?> UpdateAsync(int id, ChuyenNganhRequest request);
    Task<bool> DeleteAsync(int id);
}
