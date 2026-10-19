using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>GRADE-05: yêu cầu phúc khảo điểm cuối kỳ và điều chỉnh điểm khi được chấp nhận.</summary>
public interface IPhucKhaoService
{
    Task<PhucKhaoDto> CreateAsync(PhucKhaoRequest request);
    Task<PagedResult<PhucKhaoDto>> GetPagedAsync(PagedQuery query, TrangThaiPhucKhao? trangThai);
    Task<PhucKhaoDto> GetByIdAsync(int id);
    Task<PhucKhaoDto> XuLyAsync(int id, XuLyPhucKhaoRequest request);
}
