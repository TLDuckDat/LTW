using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Services;

/// <summary>Các truy vấn nhỏ mà nhiều service trong module StudentAffairs cùng dùng.</summary>
internal static class StudentAffairsQueries
{
    public static async Task<SinhVien> GetSinhVienAsync(this AppDbContext db, int sinhVienId) =>
        await db.Set<SinhVien>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == sinhVienId)
            ?? throw AppException.NotFound("sinh viên");

    public static async Task<HocKy> GetHocKyAsync(this AppDbContext db, int hocKyId) =>
        await db.Set<HocKy>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == hocKyId)
            ?? throw AppException.NotFound("học kỳ");

    /// <summary>Lọc theo MSSV hoặc họ tên sinh viên cho các bảng chỉ giữ SinhVienId.</summary>
    public static IQueryable<int> SinhVienPhuHop(this AppDbContext db, string? search)
    {
        var s = search!.Trim();
        return db.Set<SinhVien>().Where(x => x.MSSV.Contains(s) || x.HoTen.Contains(s)).Select(x => x.Id);
    }

    public static string XepLoaiRenLuyen(int diem) => diem switch
    {
        >= 90 => "Xuất sắc",
        >= 80 => "Tốt",
        >= 65 => "Khá",
        >= 50 => "Trung bình",
        >= 35 => "Yếu",
        _ => "Kém"
    };
}
