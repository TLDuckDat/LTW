using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Courses.Services;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Registration.Services;

public class DangKyHocPhanService(AppDbContext db, IDieuKienDangKyService dieuKien, ILopHocPhanService lopHocPhan)
    : IDangKyHocPhanService
{
    /// <summary>Lớp đạt từ ngưỡng này của sĩ số tối đa được coi là gần đầy.</summary>
    private const double NguongGanDay = 0.9;

    private readonly AppDbContext _db = db;
    private readonly IDieuKienDangKyService _dieuKien = dieuKien;
    private readonly ILopHocPhanService _lopHocPhan = lopHocPhan;

    // REG-02 Lớp học phần đang mở đăng ký trong học kỳ, kèm điều kiện đăng ký của sinh viên
    public async Task<PagedResult<LopMoDto>> GetLopMoAsync(int sinhVienId, LopMoQuery query)
    {
        if (!await _db.Set<SinhVien>().AnyAsync(x => x.Id == sinhVienId))
            throw AppException.NotFound("sinh viên hoặc học kỳ");

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
    public async Task<DangKyDto> DangKyAsync(int sinhVienId, int lopHocPhanId, bool xacNhanHocCaiThien = false)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        // Khóa dòng sinh viên rồi dòng lớp đến hết giao dịch (luôn theo thứ tự này để không deadlock):
        // - khóa lớp: hai sinh viên cùng giành chỗ cuối thì người sau chờ và thấy lớp đã đủ sĩ số;
        // - khóa sinh viên: một sinh viên đăng ký hai lớp cùng lúc không cùng vượt tín chỉ hoặc trùng lịch.
        // Serializable trước đây cũng tránh được hai trường hợp trên nhưng dễ deadlock (lỗi 1205 -> 500).
        await _db.Set<SinhVien>()
            .FromSqlInterpolated($"SELECT * FROM dbo.SinhVien WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE Id = {sinhVienId}")
            .AsNoTracking()
            .ToListAsync();
        var lop = (await _db.Set<LopHocPhan>()
                .FromSqlInterpolated($"SELECT * FROM dbo.LopHocPhan WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE Id = {lopHocPhanId}")
                .AsNoTracking()
                .ToListAsync())
            .SingleOrDefault();
        if (lop is null) throw AppException.NotFound("sinh viên hoặc lớp học phần");

        var ketQua = await _dieuKien.KiemTraAsync(sinhVienId, [lop]);
        if (ketQua is null) throw AppException.NotFound("sinh viên hoặc lớp học phần");

        var kiemTra = ketQua[lop.Id];
        if (!kiemTra.DuDieuKien)
            throw new DangKyKhongHopLeException(kiemTra.LyDo);
        if (kiemTra.LoaiDangKy == LoaiDangKy.HocCaiThien && !xacNhanHocCaiThien)
            throw new DangKyKhongHopLeException(["Môn học này đã đạt, đăng ký học cải thiện cần xác nhận (xacNhanHocCaiThien = true)."]);

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

    // REG-05 Học phần đã đăng ký (còn hiệu lực) trong học kỳ, kèm lịch học và tổng tín chỉ
    public async Task<DaDangKyDto> GetDaDangKyAsync(int sinhVienId, int? hocKyId)
    {
        if (!await _db.Set<SinhVien>().AnyAsync(x => x.Id == sinhVienId))
            throw AppException.NotFound("sinh viên");

        var hocKy = await XacDinhHocKyAsync(hocKyId);
        if (hocKy is null) return new DaDangKyDto(sinhVienId, null, 0, []);

        var cacDong = await (
            from dk in _db.Set<DangKyHocPhan>().AsNoTracking()
            join l in _db.Set<LopHocPhan>() on dk.LopHocPhanId equals l.Id
            join m in _db.Set<MonHoc>() on l.MonHocId equals m.Id
            join g in _db.Set<GiangVien>() on l.GiangVienId equals g.Id into giangVien
            from g in giangVien.DefaultIfEmpty()
            where dk.SinhVienId == sinhVienId && dk.TrangThai == TrangThaiDangKy.DaDangKy
                  && l.HocKyId == hocKy && l.TrangThai != TrangThaiLopHocPhan.DaHuy
            orderby m.Ma
            select new { DangKy = dk, Lop = l, Mon = m, TenGiangVien = g == null ? null : g.HoTen })
            .ToListAsync();

        var lopIds = cacDong.Select(x => x.Lop.Id).ToList();
        var lich = (await (
                from l in _db.Set<LichHoc>().AsNoTracking()
                join p in _db.Set<PhongHoc>() on l.PhongHocId equals p.Id into phong
                from p in phong.DefaultIfEmpty()
                where lopIds.Contains(l.LopHocPhanId)
                orderby l.Thu, l.TietBatDau
                select new LichHocDto(l.Id, l.LopHocPhanId, l.Thu, l.TietBatDau, l.SoTiet, l.PhongHocId, p == null ? null : p.Ma))
                .ToListAsync())
            .ToLookup(x => x.LopHocPhanId);

        var hocPhan = cacDong.Select(x => new HocPhanDaDangKyDto(
            x.DangKy.Id, x.Lop.Id, x.Lop.Ma, x.Mon.Ma, x.Mon.Ten, x.Mon.SoTinChi, x.TenGiangVien,
            x.DangKy.LoaiDangKy.ToString(), x.Lop.TrangThai.ToString(), lich[x.Lop.Id].ToList())).ToList();
        return new DaDangKyDto(sinhVienId, hocKy, hocPhan.Sum(x => x.SoTinChi), hocPhan);
    }

    // REG-05 Thời khóa biểu của sinh viên: các buổi học của những lớp đã đăng ký
    public async Task<ThoiKhoaBieuDto> GetThoiKhoaBieuAsync(int sinhVienId, int? hocKyId)
    {
        var daDangKy = await GetDaDangKyAsync(sinhVienId, hocKyId);

        var buoi = daDangKy.HocPhan
            .SelectMany(h => h.LichHoc.Select(l => new BuoiHocDto(
                l.Thu, l.TietBatDau, l.SoTiet, l.MaPhong, h.LopHocPhanId, h.MaLop, h.MaMonHoc, h.TenMonHoc, h.TenGiangVien)))
            .OrderBy(x => x.Thu).ThenBy(x => x.TietBatDau)
            .ToList();
        return new ThoiKhoaBieuDto(sinhVienId, daDangKy.HocKyId, buoi);
    }

    // REG-07 Theo dõi sĩ số: số đã đăng ký được đếm trực tiếp từ các lượt đăng ký còn hiệu lực
    // nên luôn khớp với thao tác đăng ký/hủy/gộp lớp, không cần cột đếm riêng
    public async Task<PagedResult<SiSoLopDto>> GetSiSoAsync(SiSoQuery query)
    {
        var trang = new PagedResult<SiSoLopDto> { Items = [], Page = query.Page, PageSize = query.PageSize };
        var hocKyId = await XacDinhHocKyAsync(query.HocKyId);
        if (hocKyId is null) return trang;

        var q = from l in _db.Set<LopHocPhan>().AsNoTracking()
                join m in _db.Set<MonHoc>() on l.MonHocId equals m.Id
                where l.HocKyId == hocKyId && l.TrangThai != TrangThaiLopHocPhan.DaHuy
                select new
                {
                    Lop = l,
                    Mon = m,
                    DaDangKy = _db.Set<DangKyHocPhan>()
                        .Count(d => d.LopHocPhanId == l.Id && d.TrangThai == TrangThaiDangKy.DaDangKy)
                };
        if (query.MonHocId is not null) q = q.Where(x => x.Mon.Id == query.MonHocId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Lop.Ma.Contains(s) || x.Mon.Ma.Contains(s) || x.Mon.Ten.Contains(s));
        }
        q = query.TinhTrang switch
        {
            TinhTrangSiSo.Day => q.Where(x => x.DaDangKy >= x.Lop.SiSoToiDa),
            TinhTrangSiSo.GanDay => q.Where(x => x.DaDangKy < x.Lop.SiSoToiDa && x.DaDangKy >= x.Lop.SiSoToiDa * NguongGanDay),
            TinhTrangSiSo.ConCho => q.Where(x => x.DaDangKy < x.Lop.SiSoToiDa * NguongGanDay),
            _ => q
        };

        trang.TotalCount = await q.CountAsync();
        var cacDong = await q.OrderBy(x => x.Mon.Ma).ThenBy(x => x.Lop.Ma)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        trang.Items = cacDong.Select(x => ToSiSoDto(x.Lop, x.Mon, x.DaDangKy)).ToList();
        return trang;
    }

    // REG-07 Cập nhật sĩ số tối đa của lớp (không nhỏ hơn số đã đăng ký, vừa sức chứa phòng)
    public async Task<SiSoLopDto> CapNhatSiSoToiDaAsync(int lopHocPhanId, int siSoToiDa)
    {
        var capNhat = await _lopHocPhan.CapNhatSiSoToiDaAsync(lopHocPhanId, siSoToiDa);

        var mon = await _db.Set<MonHoc>().AsNoTracking().FirstAsync(x => x.Id == capNhat.MonHocId);
        var lop = await _db.Set<LopHocPhan>().AsNoTracking().FirstAsync(x => x.Id == lopHocPhanId);
        return ToSiSoDto(lop, mon, capNhat.SiSoHienTai);
    }

    private static SiSoLopDto ToSiSoDto(LopHocPhan lop, MonHoc mon, int daDangKy)
    {
        var tyLe = lop.SiSoToiDa > 0 ? (double)daDangKy / lop.SiSoToiDa : 1;
        var tinhTrang = daDangKy >= lop.SiSoToiDa ? TinhTrangSiSo.Day
            : tyLe >= NguongGanDay ? TinhTrangSiSo.GanDay
            : TinhTrangSiSo.ConCho;
        return new SiSoLopDto(
            lop.Id, lop.Ma, mon.Ma, mon.Ten, lop.TrangThai.ToString(), lop.SiSoToiDa, daDangKy,
            Math.Max(0, lop.SiSoToiDa - daDangKy), Math.Round(tyLe, 4), tinhTrang.ToString());
    }

    // REG-04 Hủy đăng ký: chỉ đổi trạng thái để giữ lịch sử; giải phóng chỗ trong lớp
    public async Task<DangKyDto> HuyDangKyAsync(int dangKyId, int? chiSinhVienId)
    {
        var dangKy = await _db.Set<DangKyHocPhan>().FirstOrDefaultAsync(x => x.Id == dangKyId);
        if (dangKy is null || (chiSinhVienId is not null && dangKy.SinhVienId != chiSinhVienId))
            throw AppException.NotFound("lượt đăng ký");

        if (dangKy.TrangThai == TrangThaiDangKy.DaHuy)
            throw AppException.Conflict("Lượt đăng ký này đã được hủy trước đó.");

        var lop = await _db.Set<LopHocPhan>().AsNoTracking().FirstAsync(x => x.Id == dangKy.LopHocPhanId);
        if (lop.TrangThai != TrangThaiLopHocPhan.MoDangKy)
            throw AppException.Conflict("Lớp học phần đã bắt đầu học, kết thúc hoặc bị hủy, không thể hủy đăng ký.");
        if (await _db.Set<DiemHocPhan>().AnyAsync(x => x.DangKyHocPhanId == dangKyId))
            throw AppException.Conflict("Lượt đăng ký đã có điểm, không thể hủy.");

        // Sinh viên tự hủy phải trong thời gian của đợt đăng ký; phòng đào tạo không bị giới hạn này
        if (chiSinhVienId is not null)
        {
            var bay = DateTime.UtcNow;
            var trongDot = await _db.Set<DotDangKy>().AnyAsync(x =>
                x.HocKyId == lop.HocKyId && x.TrangThai == TrangThaiDotDangKy.DangMo
                && x.NgayBatDau <= bay && bay <= x.NgayKetThuc);
            if (!trongDot)
                throw AppException.Conflict("Đã ngoài thời gian đăng ký, sinh viên không thể tự hủy. Hãy liên hệ phòng đào tạo.");
        }

        dangKy.TrangThai = TrangThaiDangKy.DaHuy;
        await _db.SaveChangesAsync();
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

        var homNay = ThoiGian.HomNay();
        return await _db.Set<HocKy>().AsNoTracking()
            .OrderByDescending(x => x.NgayBatDau <= homNay && homNay <= x.NgayKetThuc)
            .ThenByDescending(x => x.NgayBatDau)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static DangKyDto ToDto(DangKyHocPhan x) =>
        new(x.Id, x.SinhVienId, x.LopHocPhanId, x.LoaiDangKy.ToString(), x.TrangThai.ToString(), x.CreatedAt);
}
