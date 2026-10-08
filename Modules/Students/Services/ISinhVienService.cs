using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;

namespace StudentManagement.Modules.Students.Services;

/// <summary>Nghiệp vụ module Students (Epic STU).</summary>
public interface ISinhVienService
{
    // STU-01 Danh sách sinh viên (phân trang, lọc)
    Task<object?> GetPagedAsync(PagedQuery query);
    // STU-02 Xem hồ sơ sinh viên
    Task<object?> GetByIdAsync(int id);
    // STU-03 Thêm sinh viên
    Task<object?> CreateAsync(SinhVienRequest request);
    // STU-04 Cập nhật sinh viên
    Task<object?> UpdateAsync(int id, SinhVienRequest request);
    // STU-05 Đổi trạng thái học tập
    Task<object?> ChangeStatusAsync(int id, ChangeStatusRequest request);
    // STU-06 CRUD giảng viên
    Task<object?> GetGiangVienAsync(PagedQuery query);
    // STU-07 CRUD lớp hành chính, phân cố vấn
    Task<object?> GetLopHanhChinhAsync(PagedQuery query);
}
