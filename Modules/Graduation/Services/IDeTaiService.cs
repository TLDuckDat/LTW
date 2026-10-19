using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

public interface IDeTaiService
{
    Task<PagedResult<DeTaiDto>> GetPagedAsync(PagedQuery query);
    Task<DeTaiDto> GetByIdAsync(int id);
    Task<DeTaiDto> CreateAsync(DeTaiRequest request);
    Task<DeTaiDto> UpdateAsync(int id, DeTaiRequest request);
    Task<DeTaiDto> DangKyAsync(int id, DangKyDeTaiRequest request);
    Task<DeTaiDto> PhanCongGiangVienAsync(int id, PhanCongGiangVienRequest request);
}
