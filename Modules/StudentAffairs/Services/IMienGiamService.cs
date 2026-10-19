using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;

namespace StudentManagement.Modules.StudentAffairs.Services;

/// <summary>SA-04: miễn giảm học phí theo tỷ lệ, mỗi sinh viên tối đa một mức cho mỗi học kỳ.
/// Thay đổi chỉ áp dụng vào học phí sau khi tính lại học phí của học kỳ.</summary>
public interface IMienGiamService
{
    Task<PagedResult<MienGiamDto>> GetPagedAsync(SinhVienHocKyQuery query);
    Task<MienGiamDto> GetByIdAsync(int id);
    Task<MienGiamDto> CreateAsync(MienGiamRequest request);
    Task<MienGiamDto> UpdateAsync(int id, CapNhatMienGiamRequest request);
    Task DeleteAsync(int id);
}
