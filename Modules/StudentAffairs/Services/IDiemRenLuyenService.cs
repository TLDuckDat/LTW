using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;

namespace StudentManagement.Modules.StudentAffairs.Services;

/// <summary>SA-01: điểm rèn luyện, mỗi sinh viên một điểm cho mỗi học kỳ.</summary>
public interface IDiemRenLuyenService
{
    Task<PagedResult<DiemRenLuyenDto>> GetPagedAsync(SinhVienHocKyQuery query);
    Task<DiemRenLuyenDto> GetByIdAsync(int id);
    Task<DiemRenLuyenDto> CreateAsync(DiemRenLuyenRequest request);
    Task<DiemRenLuyenDto> UpdateAsync(int id, CapNhatDiemRenLuyenRequest request);
    Task DeleteAsync(int id);
}
