using System.Data;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

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

    // REG-02 Lớp học phần đang mở đăng ký trong học kỳ, kèm điều kiện đăng ký của sinh viên
    public async Task<PagedResult<LopMoDto>?> GetLopMoAsync(int sinhVienId, LopMoQuery query)
    {
        if (!await _db.Set<SinhVien>().AnyAsync(x => x.Id == sinhVienId)) return null;

        var trang = new PagedResult<LopMoDto> { Items = [], Page = query.Page, PageSize = query.PageSize };
        var hocKyId = await XacDinhHocKyAsync(query.HocKyId);
        if (hocKyId is null) return trang;

        var q = from l in _db.Set<LopHocPhan>().AsNoTracking()
                join m in _db.Set<MonHoc>() on l.MonHocId equals m.Id
                where l.HocKyId == hocKyId && l.TrangThai == TrangThaiLopHocPhan.MoDangKy
                select new { Lop = l, Mon = m };
        if (query.MonHocId is not null) q = q.Where(x => x.Mon.Id == query.MonHocId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Lop.Ma.Contains(s) || x.Mon.Ma.Contains(s) || x.Mon.Ten.Contains(s));
        }

        trang.TotalCount = await q.CountAsync();
        var cacDong = await q.OrderBy(x => x.Mon.Ma).ThenBy(x => x.Lop.Ma)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        if (cacDong.Count == 0) return trang;

        var cacLop = cacDong.Select(x => x.Lop).ToList();
        var lopIds = cacLop.Select(l => l.Id).ToList();

        var kiemTra = (await _dieuKien.KiemTraAsync(sinhVienId, cacLop))!;
        var siSo = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => lopIds.Contains(x.LopHocPhanId) && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .GroupBy(x => x.LopHocPhanId)
            .Select(g => new { LopId = g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(x => x.LopId, x => x.SoLuong);
        var lich = (await (
                from l in _db.Set<LichHoc>().AsNoTracking()
                join p in _db.Set<PhongHoc>() on l.PhongHocId equals p.Id into phong
                from p in phong.DefaultIfEmpty()
                where lopIds.Contains(l.LopHocPhanId)
                orderby l.Thu, l.TietBatDau
                select new LichHocDto(l.Id, l.LopHocPhanId, l.Thu, l.TietBatDau, l.SoTiet, l.PhongHocId, p == null ? null : p.Ma))
                .ToListAsync())
            .ToLookup(x => x.LopHocPhanId);
        var giangVienIds = cacLop.Where(l => l.GiangVienId != null).Select(l => l.GiangVienId!.Value).Distinct().ToList();
        var tenGiangVien = await _db.Set<GiangVien>().AsNoTracking()
            .Where(x => giangVienIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.HoTen);
        var daDangKy = (await _db.Set<DangKyHocPhan>().AsNoTracking()
                .Where(x => x.SinhVienId == sinhVienId && lopIds.Contains(x.LopHocPhanId) && x.TrangThai == TrangThaiDangKy.DaDangKy)
                .Select(x => x.LopHocPhanId)
                .ToListAsync())
            .ToHashSet();

        trang.Items = cacDong.Select(x =>
        {
            var kq = kiemTra[x.Lop.Id];
            var hienTai = siSo.GetValueOrDefault(x.Lop.Id);
            return new LopMoDto(
                x.Lop.Id, x.Lop.Ma, x.Lop.HocKyId, x.Mon.Id, x.Mon.Ma, x.Mon.Ten, x.Mon.SoTinChi,
                x.Lop.GiangVienId, x.Lop.GiangVienId is { } gv ? tenGiangVien.GetValueOrDefault(gv) : null,
                x.Lop.SiSoToiDa, hienTai, Math.Max(0, x.Lop.SiSoToiDa - hienTai),
                lich[x.Lop.Id].ToList(), daDangKy.Contains(x.Lop.Id), kq.DuDieuKien, kq.LoaiDangKy.ToString(), kq.LyDo);
        }).ToList();
        return trang;
    }

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

    /// <summary>
    /// Học kỳ được chọn: theo yêu cầu, nếu không thì học kỳ có đợt đăng ký đang mở, rồi học kỳ đang diễn ra,
    /// cuối cùng là học kỳ gần nhất.
    /// </summary>
    private async Task<int?> XacDinhHocKyAsync(int? hocKyId)
    {
        if (hocKyId is not null)
            return await _db.Set<HocKy>().AnyAsync(x => x.Id == hocKyId) ? hocKyId : null;

        var dotDangMo = await _db.Set<DotDangKy>().AsNoTracking()
            .Where(x => x.TrangThai == TrangThaiDotDangKy.DangMo)
            .OrderByDescending(x => x.NgayBatDau)
            .Select(x => (int?)x.HocKyId)
            .FirstOrDefaultAsync();
        if (dotDangMo is not null) return dotDangMo;

        var homNay = DateOnly.FromDateTime(DateTime.Today);
        return await _db.Set<HocKy>().AsNoTracking()
            .OrderByDescending(x => x.NgayBatDau <= homNay && homNay <= x.NgayKetThuc)
            .ThenByDescending(x => x.NgayBatDau)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static DangKyDto ToDto(DangKyHocPhan x) =>
        new(x.Id, x.SinhVienId, x.LopHocPhanId, x.LoaiDangKy.ToString(), x.TrangThai.ToString(), x.CreatedAt);
}
