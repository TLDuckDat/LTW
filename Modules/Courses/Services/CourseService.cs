using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

public class CourseService(AppDbContext db) : ICourseService
{
    private readonly AppDbContext _db = db;

    // CRS-01 CRUD môn học
    public Task<object?> GetMonHocAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // CRS-02 Quản lý môn tiên quyết
    public Task<object?> AddTienQuyetAsync(int id, TienQuyetRequest request) => throw new NotImplementedException("TODO");

    // CRS-03 CRUD phòng học
    public Task<object?> GetPhongHocAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // CRS-04 Mở lớp học phần
    public Task<object?> CreateLopHocPhanAsync(LopHocPhanRequest request) => throw new NotImplementedException("TODO");

    // CRS-05 Xếp lịch học, kiểm tra trùng phòng/giờ
    public Task<object?> AddLichHocAsync(int id, LichHocRequest request) => throw new NotImplementedException("TODO");
}
