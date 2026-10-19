using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

public interface ICoSoThucTapService
{
    Task<PagedResult<CoSoThucTapDto>> GetPagedAsync(PagedQuery query);
    Task<CoSoThucTapDto> GetByIdAsync(int id);
    Task<CoSoThucTapDto> CreateAsync(CoSoThucTapRequest request);
    Task<CoSoThucTapDto> UpdateAsync(int id, CoSoThucTapRequest request);
    Task<CoSoThucTapDto> SetTrangThaiAsync(int id, TrangThaiCoSoRequest request);
    Task DeleteAsync(int id);
}
