using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;

namespace StudentManagement.Modules.Students.Services;

/// <summary>Nghiệp vụ sinh viên (STU-01, STU-02, STU-03, STU-06, STU-07).</summary>
public interface ISinhVienService
{
    // STU-01 & STU-03: Danh sách sinh viên (phân trang, lọc đa tiêu chí)
    Task<PagedResult<SinhVienDto>> GetPagedAsync(SinhVienFilterQuery query);

    // STU-02: Xem chi tiết hồ sơ sinh viên
    Task<SinhVienDetailDto> GetByIdAsync(int id);

    // STU-01: Thêm sinh viên
    Task<SinhVienDto> CreateAsync(CreateSinhVienRequest request);

    // STU-01: Cập nhật thông tin sinh viên
    Task<SinhVienDto> UpdateAsync(int id, UpdateSinhVienRequest request);

    // STU-01: Xóa sinh viên
    Task DeleteAsync(int id);

    // STU-04: Chuyển lớp hành chính cho 1 sinh viên
    Task ChuyenLopAsync(int id, int? lopHanhChinhId);

    // STU-06: Đổi trạng thái học tập
    Task<SinhVienDto> ChangeStatusAsync(int id, ChangeStatusRequest request);

    // STU-07: Tiếp nhận bảo lưu
    Task<SinhVienDto> BaoLuuAsync(int id, BaoLuuRequest request);

    // STU-07: Buộc thôi học / xin thôi học
    Task<SinhVienDto> ThoiHocAsync(int id, ThoiHocRequest request);

    // STU-07: Quay lại học tập
    Task<SinhVienDto> QuayLaiHocAsync(int id, string? ghiChu);

    // STU-07: Xem lịch sử học tập
    Task<List<LichSuHocTapDto>> GetLichSuHocTapAsync(int id);
}
