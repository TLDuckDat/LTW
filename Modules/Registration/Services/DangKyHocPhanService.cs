using System.Data;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Registration.Services;

public class DangKyHocPhanService(AppDbContext db, IDieuKienDangKyService dieuKien) : IDangKyHocPhanService
{
    private readonly AppDbContext _db = db;
    private readonly IDieuKienDangKyService _dieuKien = dieuKien;

    public Task<int?> GetSinhVienIdCuaTaiKhoanAsync(int taiKhoanId)
        => _db.Set<TaiKhoan>().AsNoTracking()
            .Where(x => x.Id == taiKhoanId)
            .Select(x => x.SinhVienId)
            .FirstOrDefaultAsync();

    // REG-03 Đăng ký học phần: kiểm tra toàn bộ điều kiện rồi mới ghi, trong một giao dịch
    public async Task<DangKyDto?> DangKyAsync(int sinhVienId, int lopHocPhanId)
    {
        // Serializable để hai sinh viên cùng đăng ký chỗ cuối cùng không cùng vượt sĩ số
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var lop = await _db.Set<LopHocPhan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lopHocPhanId);
        if (lop is null) return null;

        var ketQua = await _dieuKien.KiemTraAsync(sinhVienId, [lop]);
        if (ketQua is null) return null;

        var kiemTra = ketQua[lop.Id];
        if (!kiemTra.DuDieuKien)
            throw new DangKyKhongHopLeException(kiemTra.LyDo);

        // Cặp (sinh viên, lớp) là duy nhất: đăng ký lại sau khi đã hủy thì dùng lại dòng cũ
        var dangKy = await _db.Set<DangKyHocPhan>()
            .FirstOrDefaultAsync(x => x.SinhVienId == sinhVienId && x.LopHocPhanId == lopHocPhanId);
        if (dangKy is null)
        {
            dangKy = new DangKyHocPhan { SinhVienId = sinhVienId, LopHocPhanId = lopHocPhanId };
            _db.Add(dangKy);
        }
        dangKy.LoaiDangKy = kiemTra.LoaiDangKy;
        dangKy.TrangThai = TrangThaiDangKy.DaDangKy;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return ToDto(dangKy);
    }

    private static DangKyDto ToDto(DangKyHocPhan x) =>
        new(x.Id, x.SinhVienId, x.LopHocPhanId, x.LoaiDangKy.ToString(), x.TrangThai.ToString(), x.CreatedAt);
}
