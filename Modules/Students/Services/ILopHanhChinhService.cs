using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;

namespace StudentManagement.Modules.Students.Services;

/// <summary>Nghiệp vụ lớp hành chính và phân lớp (STU-04, STU-05).</summary>
public interface ILopHanhChinhService
{
    Task<PagedResult<LopHanhChinhDto>> GetPagedAsync(PagedQuery query, int? nganhId = null, int? khoaTuyen = null);
    Task<LopHanhChinhDetailDto> GetByIdAsync(int id);
    Task<LopHanhChinhDto> CreateAsync(LopHanhChinhRequest request);
    Task<LopHanhChinhDto> UpdateAsync(int id, LopHanhChinhRequest request);
    Task DeleteAsync(int id);
    Task PhanLopAsync(int id, PhanLopRequest request);
    Task PhanCoVanAsync(int id, PhanCoVanRequest request);
}
