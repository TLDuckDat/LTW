using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

/// <summary>Nghiệp vụ module Graduation (Epic GRAD).</summary>
public interface IGraduationService
{
    Task<XetTotNghiepDto> XetTotNghiepAsync(int sinhVienId);
    Task<ChiTietXetTotNghiepDto> GetBySinhVienIdAsync(int sinhVienId);
    Task<PagedResult<XetTotNghiepDto>> GetPagedAsync(XetTotNghiepQuery query);
}
