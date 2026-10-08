using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Nghiệp vụ module Courses (Epic CRS).</summary>
public interface ICourseService
{
    // CRS-01 CRUD môn học
    Task<object?> GetMonHocAsync(PagedQuery query);
    // CRS-02 Quản lý môn tiên quyết
    Task<object?> AddTienQuyetAsync(int id, TienQuyetRequest request);
    // CRS-03 CRUD phòng học
    Task<object?> GetPhongHocAsync(PagedQuery query);
    // CRS-04 Mở lớp học phần
    Task<object?> CreateLopHocPhanAsync(LopHocPhanRequest request);
    // CRS-05 Xếp lịch học, kiểm tra trùng phòng/giờ
    Task<object?> AddLichHocAsync(int id, LichHocRequest request);
}
