using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Students.Dtos;

namespace StudentManagement.Modules.Students.Services;

public class SinhVienService(AppDbContext db) : ISinhVienService
{
    private readonly AppDbContext _db = db;

    // STU-01 Danh sách sinh viên (phân trang, lọc)
    public Task<object?> GetPagedAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // STU-02 Xem hồ sơ sinh viên
    public Task<object?> GetByIdAsync(int id) => throw new NotImplementedException("TODO");

    // STU-03 Thêm sinh viên
    public Task<object?> CreateAsync(SinhVienRequest request) => throw new NotImplementedException("TODO");

    // STU-04 Cập nhật sinh viên
    public Task<object?> UpdateAsync(int id, SinhVienRequest request) => throw new NotImplementedException("TODO");

    // STU-05 Đổi trạng thái học tập
    public Task<object?> ChangeStatusAsync(int id, ChangeStatusRequest request) => throw new NotImplementedException("TODO");

    // STU-06 CRUD giảng viên
    public Task<object?> GetGiangVienAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // STU-07 CRUD lớp hành chính, phân cố vấn
    public Task<object?> GetLopHanhChinhAsync(PagedQuery query) => throw new NotImplementedException("TODO");
}
