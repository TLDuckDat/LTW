using StudentManagement.Common;
using StudentManagement.Modules.Reports.Dtos;

namespace StudentManagement.Modules.Reports.Services;

/// <summary>RPT-05 Thông báo. chiDaDang = true: chỉ thấy thông báo đã đến giờ đăng (người dùng thường).</summary>
public interface IThongBaoService
{
    Task<PagedResult<ThongBaoDto>> GetPagedAsync(PagedQuery query, bool chiDaDang);
    Task<ThongBaoDto> GetByIdAsync(int id, bool chiDaDang);
    Task<ThongBaoDto> CreateAsync(ThongBaoRequest request, int nguoiTaoId);
    Task<ThongBaoDto> UpdateAsync(int id, ThongBaoRequest request);
    Task DeleteAsync(int id);
}
