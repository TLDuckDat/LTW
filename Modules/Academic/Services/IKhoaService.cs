using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>MẪU CRUD: quản lý Khoa (ACA-01).</summary>
public interface IKhoaService
{
    Task<PagedResult<KhoaDto>> GetPagedAsync(PagedQuery query);
    Task<KhoaDto> GetByIdAsync(int id);
    Task<KhoaDto> CreateAsync(KhoaRequest request);
    Task<KhoaDto> UpdateAsync(int id, KhoaRequest request);
    Task DeleteAsync(int id);
}
