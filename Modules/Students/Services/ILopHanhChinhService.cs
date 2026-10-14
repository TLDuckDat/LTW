using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;

namespace StudentManagement.Modules.Students.Services;

/// <summary>Nghiệp vụ lớp hành chính và phân lớp (STU-04, STU-05).</summary>
public interface ILopHanhChinhService
{
    Task<PagedResult<LopHanhChinhDto>> GetPagedAsync(PagedQuery query, int? nganhId = null, int? khoaTuyen = null);
    Task<LopHanhChinhDetailDto?> GetByIdAsync(int id);
    Task<LopHanhChinhDto> CreateAsync(LopHanhChinhRequest request);
    Task<LopHanhChinhDto?> UpdateAsync(int id, LopHanhChinhRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> PhanLopAsync(int id, PhanLopRequest request);
    Task<bool> PhanCoVanAsync(int id, PhanCoVanRequest request);
}
