using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;

namespace StudentManagement.Modules.StudentAffairs.Services;

/// <summary>
/// SA-05: xét học bổng khuyến khích theo học kỳ. Điều kiện: đang học, điểm rèn luyện và GPA học kỳ chính thức
/// đạt ngưỡng (quy chế HB_*), không bị kỷ luật có ngày quyết định trong học kỳ. Xét lại không cấp trùng.
/// </summary>
public interface IHocBongService
{
    Task<PagedResult<HocBongDto>> GetPagedAsync(SinhVienHocKyQuery query);
    Task<XetHocBongResultDto> XetHocBongAsync(int hocKyId);
    /// <summary>Thu hồi (xóa) học bổng đã cấp.</summary>
    Task DeleteAsync(int id);
}
