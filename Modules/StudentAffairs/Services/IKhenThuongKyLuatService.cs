using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;

namespace StudentManagement.Modules.StudentAffairs.Services;

/// <summary>SA-06: quyết định khen thưởng / kỷ luật sinh viên.</summary>
public interface IKhenThuongKyLuatService
{
    Task<PagedResult<KhenThuongKyLuatDto>> GetPagedAsync(KhenThuongKyLuatQuery query);
    Task<KhenThuongKyLuatDto> GetByIdAsync(int id);
    Task<KhenThuongKyLuatDto> CreateAsync(KhenThuongKyLuatRequest request);
    Task<KhenThuongKyLuatDto> UpdateAsync(int id, KhenThuongKyLuatRequest request);
    Task DeleteAsync(int id);
}
