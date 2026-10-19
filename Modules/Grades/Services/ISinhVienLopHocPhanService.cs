using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>GRADE-01: danh sách sinh viên của lớp học phần; thêm, chuyển lớp, hủy đăng ký.</summary>
public interface ISinhVienLopHocPhanService
{
    Task<PagedResult<SinhVienTrongLopDto>> GetDanhSachAsync(int lopHocPhanId, PagedQuery query);
    Task<DangKyHocPhanDto> ThemSinhVienAsync(int lopHocPhanId, ThemSinhVienRequest request);
    Task<DangKyHocPhanDto> ChuyenLopAsync(int lopHocPhanId, int dangKyHocPhanId, ChuyenLopRequest request);
    /// <summary>Hủy lượt đăng ký: chỉ đổi trạng thái, không xóa để giữ lịch sử.</summary>
    Task HuyDangKyAsync(int lopHocPhanId, int dangKyHocPhanId);
}
