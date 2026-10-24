using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Reports.Dtos;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Reports.Services;

public class ReportService(AppDbContext db, IBangDiemService bangDiem, ICanhBaoHocVuService canhBao) : IReportService
{
    /// <summary>Thứ tự hiển thị điểm chữ, từ cao xuống thấp.</summary>
    private static readonly string[] CacDiemChu = ["A+", "A", "B+", "B", "C+", "C", "D+", "D", "F+", "F"];

    private readonly AppDbContext _db = db;
    private readonly IBangDiemService _bangDiem = bangDiem;
    private readonly ICanhBaoHocVuService _canhBao = canhBao;

    // RPT-01 Thống kê sinh viên theo khoa/ngành/trạng thái
    public async Task<ThongKeSinhVienDto> ThongKeSinhVienAsync()
    {
        var sinhVien = _db.Set<SinhVien>().AsNoTracking();

        var theoNganh = await _db.Set<Nganh>().AsNoTracking()
            .OrderBy(n => n.Ma)
            .Select(n => new ThongKeItemDto(n.Id, n.Ma, n.Ten, sinhVien.Count(s => s.NganhId == n.Id)))
            .ToListAsync();

        // Sinh viên thuộc khoa thông qua ngành
        var theoKhoa = await _db.Set<Khoa>().AsNoTracking()
            .OrderBy(k => k.Ma)
            .Select(k => new ThongKeItemDto(k.Id, k.Ma, k.Ten,
                sinhVien.Count(s => _db.Set<Nganh>().Any(n => n.Id == s.NganhId && n.KhoaId == k.Id))))
            .ToListAsync();

        var demTrangThai = await sinhVien
            .GroupBy(s => s.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(x => x.TrangThai, x => x.SoLuong);
        var theoTrangThai = Enum.GetValues<TrangThaiSinhVien>()
            .Select(t => new ThongKeItemDto(null, t.ToString(), t.ToString(), demTrangThai.GetValueOrDefault(t)))
            .ToList();

        return new ThongKeSinhVienDto(demTrangThai.Values.Sum(), theoKhoa, theoNganh, theoTrangThai);
    }

    // RPT-02 Thống kê kết quả học tập theo học kỳ (dùng lại cách tính GPA của module Grades)
    public async Task<ThongKeKetQuaHocTapDto> ThongKeKetQuaAsync(int hocKyId)
    {
        var hocKy = await GetHocKyAsync(hocKyId);

        var sinhVienIds = await SinhVienDangKyTrongHocKy(hocKyId).ToArrayAsync();
        var bangDiem = await _bangDiem.GetBangDiemNhieuSinhVienAsync(sinhVienIds);
        var ketQuaHocKy = bangDiem.Values
            .Select(b => b.DanhSachHocKy.FirstOrDefault(x => x.HocKyId == hocKyId))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        var gpaChinhThuc = ketQuaHocKy
            .Where(x => x.Gpa is not null && !x.GpaTamTinh)
            .Select(x => x.Gpa!.Value)
            .ToList();
        var theoXepLoai = GradeCalculator.ThangXepLoai
            .Select(loai => new ThongKeItemDto(null, loai.Ma, loai.Ten,
                gpaChinhThuc.Count(gpa => GradeCalculator.XepLoai(gpa).Ma == loai.Ma)))
            .ToList();

        var monHoc = ketQuaHocKy.SelectMany(x => x.DanhSachMonHoc).ToList();
        var coKetQua = monHoc.Where(x => x.DaKhoa && x.DiemTongKet is not null && x.DiemChu is not null).ToList();
        var demDiemChu = coKetQua.GroupBy(x => x.DiemChu!).ToDictionary(g => g.Key, g => g.Count());
        var theoDiemChu = CacDiemChu
            .Select(d => new ThongKeItemDto(null, d, d, demDiemChu.GetValueOrDefault(d)))
            .ToList();

        var theoMonHoc = monHoc
            .GroupBy(x => new { x.MonHocId, x.MaMonHoc, x.TenMonHoc, x.SoTinChi })
            .OrderBy(g => g.Key.MaMonHoc)
            .Select(g =>
            {
                var ketQua = g.Where(x => x.DaKhoa && x.DiemTongKet is not null).ToList();
                var soDat = ketQua.Count(x => GradeCalculator.LaDiemDat(x.DiemTongKet!.Value));
                return new KetQuaMonHocDto(
                    g.Key.MonHocId, g.Key.MaMonHoc, g.Key.TenMonHoc, g.Key.SoTinChi,
                    g.Count(), ketQua.Count, soDat,
                    ketQua.Count == 0 ? null : Math.Round((decimal)soDat / ketQua.Count, 4),
                    ketQua.Count == 0 ? null : Math.Round(ketQua.Average(x => x.DiemTongKet!.Value), 2));
            })
            .ToList();

        return new ThongKeKetQuaHocTapDto(
            hocKy.Id, hocKy.Ten, hocKy.TenNamHoc,
            sinhVienIds.Length, gpaChinhThuc.Count, sinhVienIds.Length - gpaChinhThuc.Count,
            gpaChinhThuc.Count == 0 ? null : Math.Round(gpaChinhThuc.Average(), 2),
            theoXepLoai, theoDiemChu, theoMonHoc);
    }

    // RPT-03 Báo cáo công nợ học phí của học kỳ
    public async Task<BaoCaoCongNoDto> BaoCaoCongNoAsync(int hocKyId, PagedQuery query)
    {
        var hocKy = await GetHocKyAsync(hocKyId);
        var hocPhi = _db.Set<HocPhi>().AsNoTracking().Where(x => x.HocKyId == hocKyId);

        var tong = await hocPhi
            .GroupBy(_ => 1)
            .Select(g => new
            {
                SoKhoan = g.Count(),
                PhaiThu = g.Sum(x => x.SoTien),
                DaThu = g.Sum(x => x.SoTienDaNop < x.SoTien ? x.SoTienDaNop : x.SoTien),
                ConNo = g.Sum(x => x.SoTien > x.SoTienDaNop ? x.SoTien - x.SoTienDaNop : 0m),
                CanHoan = g.Sum(x => x.SoTienDaNop > x.SoTien ? x.SoTienDaNop - x.SoTien : 0m)
            })
            .FirstOrDefaultAsync();

        var conNo = hocPhi.Where(x => x.SoTien > x.SoTienDaNop);
        var soSinhVienConNo = await conNo.Select(x => x.SinhVienId).Distinct().CountAsync();
        var soChuaTinhHocPhi = await SinhVienDangKyTrongHocKy(hocKyId)
            .CountAsync(id => !hocPhi.Any(h => h.SinhVienId == id));

        var danhSach = from hp in conNo
                       join sv in _db.Set<SinhVien>().AsNoTracking() on hp.SinhVienId equals sv.Id
                       select new { hp, sv };
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            danhSach = danhSach.Where(x => x.sv.MSSV.Contains(search) || x.sv.HoTen.Contains(search));
        }
        var trang = await danhSach.OrderBy(x => x.sv.MSSV).ThenBy(x => x.hp.Id)
            .Select(x => new CongNoSinhVienDto(x.hp.Id, x.sv.Id, x.sv.MSSV, x.sv.HoTen,
                x.hp.SoTien, x.hp.SoTienDaNop, x.hp.SoTien - x.hp.SoTienDaNop))
            .ToPagedResultAsync(query);

        return new BaoCaoCongNoDto(
            hocKy.Id, hocKy.Ten, tong?.SoKhoan ?? 0, soSinhVienConNo, soChuaTinhHocPhi,
            tong?.PhaiThu ?? 0m, tong?.DaThu ?? 0m, tong?.ConNo ?? 0m, tong?.CanHoan ?? 0m, trang);
    }

    // RPT-04 Danh sách sinh viên cảnh báo học vụ: dùng chung quy tắc của module Grades
    public Task<PagedResult<CanhBaoHocVuDto>> CanhBaoHocVuAsync(int hocKyId, PagedQuery query)
        => _canhBao.GetDanhSachCanhBaoAsync(hocKyId, query);

    // RPT-05 Số liệu tổng quan: học kỳ mốc là học kỳ đang diễn ra, nếu không có thì học kỳ mới nhất
    public async Task<TongQuanDto> TongQuanAsync()
    {
        var thongKe = await ThongKeSinhVienAsync();
        var today = ThoiGian.HomNay();

        var dangDienRa = await (from hk in _db.Set<HocKy>().AsNoTracking()
                                join nh in _db.Set<NamHoc>().AsNoTracking() on hk.NamHocId equals nh.Id
                                where hk.NgayBatDau <= today && today <= hk.NgayKetThuc
                                select new HocKyBaoCao(hk.Id, hk.Ten, nh.Ten)).FirstOrDefaultAsync();
        var hocKy = dangDienRa ?? await (from hk in _db.Set<HocKy>().AsNoTracking()
                                         join nh in _db.Set<NamHoc>().AsNoTracking() on hk.NamHocId equals nh.Id
                                         orderby hk.NgayBatDau descending
                                         select new HocKyBaoCao(hk.Id, hk.Ten, nh.Ten)).FirstOrDefaultAsync();

        var gpa = hocKy is null ? null : (await ThongKeKetQuaAsync(hocKy.Id)).GpaTrungBinh;
        var tongSo = thongKe.TongSoSinhVien;
        var dangHoc = thongKe.TheoTrangThai.FirstOrDefault(x => x.Ma == nameof(TrangThaiSinhVien.DangHoc))?.SoLuong ?? 0;
        var daTotNghiep = thongKe.TheoTrangThai.FirstOrDefault(x => x.Ma == nameof(TrangThaiSinhVien.DaTotNghiep))?.SoLuong ?? 0;
        var tyLe = tongSo == 0 ? 0m : Math.Round(daTotNghiep / (decimal)tongSo, 4);

        return new TongQuanDto(
            tongSo,
            dangHoc,
            tyLe,
            gpa,
            hocKy is null ? null : new HocKyTongQuanDto(hocKy.Id, hocKy.Ten, hocKy.TenNamHoc),
            thongKe.TheoKhoa,
            thongKe.TheoTrangThai);
    }

    private sealed record HocKyBaoCao(int Id, string Ten, string TenNamHoc);

    private async Task<HocKyBaoCao> GetHocKyAsync(int hocKyId) =>
        await (from hk in _db.Set<HocKy>().AsNoTracking()
               join nh in _db.Set<NamHoc>().AsNoTracking() on hk.NamHocId equals nh.Id
               where hk.Id == hocKyId
               select new HocKyBaoCao(hk.Id, hk.Ten, nh.Ten))
            .FirstOrDefaultAsync()
        ?? throw AppException.NotFound("học kỳ");

    /// <summary>Sinh viên có lượt đăng ký còn hiệu lực trong lớp (chưa hủy) của học kỳ.</summary>
    private IQueryable<int> SinhVienDangKyTrongHocKy(int hocKyId) =>
        (from dk in _db.Set<DangKyHocPhan>().AsNoTracking()
         join lop in _db.Set<LopHocPhan>().AsNoTracking() on dk.LopHocPhanId equals lop.Id
         where lop.HocKyId == hocKyId && lop.TrangThai != TrangThaiLopHocPhan.DaHuy
               && dk.TrangThai == TrangThaiDangKy.DaDangKy
         select dk.SinhVienId).Distinct();
}
