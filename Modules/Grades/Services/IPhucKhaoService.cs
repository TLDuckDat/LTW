using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>
/// GRADE-05: yêu cầu phúc khảo điểm cuối kỳ và điều chỉnh điểm khi được chấp nhận.
/// chiSinhVienId: khi có giá trị, chỉ thấy/tạo phúc khảo trên điểm của chính sinh viên đó.
/// </summary>
public interface IPhucKhaoService
{
    Task<PhucKhaoDto> CreateAsync(PhucKhaoRequest request, int? chiSinhVienId = null);
    Task<PagedResult<PhucKhaoDto>> GetPagedAsync(PagedQuery query, TrangThaiPhucKhao? trangThai, int? chiSinhVienId = null);
    Task<PhucKhaoDto> GetByIdAsync(int id, int? chiSinhVienId = null);
    Task<PhucKhaoDto> XuLyAsync(int id, XuLyPhucKhaoRequest request);
}
