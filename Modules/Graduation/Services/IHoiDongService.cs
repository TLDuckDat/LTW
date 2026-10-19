using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

public interface IHoiDongService
{
    Task<PagedResult<HoiDongDto>> GetPagedAsync(PagedQuery query);
    Task<HoiDongDto> GetByIdAsync(int id);
    Task<HoiDongDto> CreateAsync(HoiDongRequest request);
    Task<HoiDongDto> UpdateAsync(int id, HoiDongRequest request);
    Task<PagedResult<DeTaiHoiDongDto>> GetDeTaiAsync(int id, PagedQuery query);
    Task<PhanCongDeTaiDto> PhanCongDeTaiAsync(int id, int deTaiId);
}
