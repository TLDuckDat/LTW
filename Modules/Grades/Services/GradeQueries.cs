using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>Các truy vấn nhỏ mà nhiều service trong module Grades cùng dùng.</summary>
internal static class GradeQueries
{
    /// <summary>Lấy lớp học phần (chỉ đọc). Báo lỗi 404 "Không tìm thấy {tenHienThi}." nếu không có.</summary>
    public static async Task<LopHocPhan> GetLopHocPhanAsync(
        this AppDbContext db, int lopHocPhanId, string tenHienThi = "lớp học phần") =>
        await db.Set<LopHocPhan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lopHocPhanId)
            ?? throw AppException.NotFound(tenHienThi);
}
