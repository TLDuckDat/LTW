using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

public interface IKetQuaBaoVeService
{
    Task<PagedResult<KetQuaBaoVeDto>> GetPagedAsync(KetQuaBaoVeQuery query);
    Task<KetQuaBaoVeDto> GetByDeTaiIdAsync(int deTaiId);
    Task<KetQuaBaoVeDto> SaveAsync(int deTaiId, NhapKetQuaBaoVeRequest request);
}
