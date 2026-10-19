using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>GRADE-07: đánh giá cảnh báo học vụ theo học kỳ chính. Đánh giá lại mỗi lần gọi
/// để kết quả phúc khảo được phản ánh ngay.</summary>
public interface ICanhBaoHocVuService
{
    Task<CanhBaoHocVuDto> GetCanhBaoAsync(int sinhVienId, int hocKyId);

    /// <summary>Chỉ trả về các sinh viên đã đủ dữ liệu và bị cảnh báo.</summary>
    Task<PagedResult<CanhBaoHocVuDto>> GetDanhSachCanhBaoAsync(int hocKyId, PagedQuery query);
}
