using StudentManagement.Common;
using StudentManagement.Modules.Registration.Dtos;

namespace StudentManagement.Modules.Registration.Services;

/// <summary>Quản lý đợt đăng ký học phần (REG-01).</summary>
public interface IDotDangKyService
{
    Task<PagedResult<DotDangKyDto>> GetPagedAsync(DotDangKyQuery query);
    Task<DotDangKyDto?> GetByIdAsync(int id);
    Task<DotDangKyDto> CreateAsync(DotDangKyRequest request);
    Task<DotDangKyDto?> UpdateAsync(int id, DotDangKyUpdateRequest request);
    Task<bool> DeleteAsync(int id);
    Task<DotDangKyDto?> MoAsync(int id);
    Task<DotDangKyDto?> DongAsync(int id);
}
