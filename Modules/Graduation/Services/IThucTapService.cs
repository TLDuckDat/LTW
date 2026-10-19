using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

public interface IThucTapService
{
    Task<PagedResult<ThucTapDto>> GetPagedAsync(ThucTapQuery query);
    Task<ThucTapDto> GetByIdAsync(int id);
    Task<ThucTapDto> CreateAsync(ThucTapRequest request);
    Task<ThucTapDto> UpdateAsync(int id, CapNhatThucTapRequest request);
    Task<ThucTapDto> HuyAsync(int id);
    Task<KetQuaThucTapDto?> GetKetQuaAsync(int id);
    Task<KetQuaThucTapDto> SetKetQuaAsync(int id, KetQuaThucTapRequest request);
}
