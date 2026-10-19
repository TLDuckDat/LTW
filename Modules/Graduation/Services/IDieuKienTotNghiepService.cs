using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

public interface IDieuKienTotNghiepService
{
    Task<DieuKienTotNghiepDto> GetBySinhVienIdAsync(int sinhVienId);
    Task<PagedResult<DieuKienTotNghiepDto>> GetPagedAsync(PagedQuery query);
}
