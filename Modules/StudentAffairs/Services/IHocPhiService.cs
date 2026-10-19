using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;

namespace StudentManagement.Modules.StudentAffairs.Services;

/// <summary>
/// SA-02, SA-03: tính học phí theo tín chỉ đăng ký (trừ miễn giảm) và ghi nhận các lần nộp.
/// chiSinhVienId: khi có giá trị, chỉ thấy học phí của chính sinh viên đó (404 nếu không phải).
/// </summary>
public interface IHocPhiService
{
    Task<PagedResult<HocPhiDto>> GetPagedAsync(HocPhiQuery query);
    Task<HocPhiDto> GetByIdAsync(int id, int? chiSinhVienId = null);
    Task<TinhHocPhiResultDto> TinhHocPhiAsync(int hocKyId);
    Task<ThanhToanResultDto> ThanhToanAsync(int id, ThanhToanRequest? request, int nguoiThuId);
    Task<IReadOnlyList<ThanhToanHocPhiDto>> GetLichSuThanhToanAsync(int id, int? chiSinhVienId = null);
}
