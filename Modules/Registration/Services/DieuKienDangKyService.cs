using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Academic.Services;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Grades.Services;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Registration.Services;

public class DieuKienDangKyService(AppDbContext db) : IDieuKienDangKyService
{
    /// <summary>Số tín chỉ tối đa mỗi học kỳ khi chưa cấu hình quy chế TIN_CHI_TOI_DA_HOC_KY.</summary>
    public const int TinChiToiDaMacDinh = 24;

    private readonly AppDbContext _db = db;

    private sealed record DangKyDaCo(
        int LopHocPhanId, string MaLop, int MonHocId, string MaMon, int SoTinChi, int HocKyId, DateOnly NgayBatDauHocKy, bool DaDat);

    private sealed record Buoi(int LopHocPhanId, int Thu, int TietBatDau, int SoTiet)
    {
        public int TietKetThuc => TietBatDau + SoTiet - 1;
        public bool Giao(Buoi khac) => Thu == khac.Thu && TietBatDau <= khac.TietKetThuc && khac.TietBatDau <= TietKetThuc;
    }

    public async Task<Dictionary<int, KetQuaKiemTraDangKy>?> KiemTraAsync(int sinhVienId, IReadOnlyList<LopHocPhan> cacLop)
    {
        var sinhVien = await _db.Set<SinhVien>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == sinhVienId);
        if (sinhVien is null) return null;
        var tinChiToiDa = await _db.GetThamSoNguyenAsync(MaQuyChe.TinChiToiDaHocKy, TinChiToiDaMacDinh);

        var ketQua = cacLop.ToDictionary(l => l.Id, _ => new KetQuaKiemTraDangKy());
        if (cacLop.Count == 0) return ketQua;

        var lopIds = cacLop.Select(l => l.Id).ToList();
        var monIds = cacLop.Select(l => l.MonHocId).Distinct().ToList();
        var hocKyIds = cacLop.Select(l => l.HocKyId).Distinct().ToList();

        // Đợt đăng ký đang mở và còn trong thời gian (giờ máy chủ)
        var bay = DateTime.Now;
        var hocKyDangMo = (await _db.Set<DotDangKy>().AsNoTracking()
                .Where(x => hocKyIds.Contains(x.HocKyId) && x.TrangThai == TrangThaiDotDangKy.DangMo
                            && x.NgayBatDau <= bay && bay <= x.NgayKetThuc)
                .Select(x => x.HocKyId)
                .ToListAsync())
            .ToHashSet();

        var daCo = await (
            from dk in _db.Set<DangKyHocPhan>().AsNoTracking()
            join l in _db.Set<LopHocPhan>() on dk.LopHocPhanId equals l.Id
            join m in _db.Set<MonHoc>() on l.MonHocId equals m.Id
            join hk in _db.Set<HocKy>() on l.HocKyId equals hk.Id
            join d in _db.Set<DiemHocPhan>() on dk.Id equals d.DangKyHocPhanId into nhomDiem
            from d in nhomDiem.DefaultIfEmpty()
            where dk.SinhVienId == sinhVienId && dk.TrangThai == TrangThaiDangKy.DaDangKy
                  && l.TrangThai != TrangThaiLopHocPhan.DaHuy
            select new DangKyDaCo(l.Id, l.Ma, l.MonHocId, m.Ma, m.SoTinChi, l.HocKyId, hk.NgayBatDau,
                d != null && d.DaKhoa && d.DiemTongKet >= GradeCalculator.DiemDat))
            .ToListAsync();
        var ngayBatDauHocKy = await _db.Set<HocKy>().AsNoTracking()
            .Where(x => hocKyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.NgayBatDau);
        var monDaDat = daCo.Where(x => x.DaDat).Select(x => x.MonHocId).ToHashSet();

        var tinChiMon = await _db.Set<MonHoc>().AsNoTracking()
            .Where(x => monIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.SoTinChi);

        var tienQuyet = (await (
                from tq in _db.Set<MonHocTienQuyet>().AsNoTracking()
                join m in _db.Set<MonHoc>() on tq.TienQuyetId equals m.Id
                where monIds.Contains(tq.MonHocId)
                select new { tq.MonHocId, tq.TienQuyetId, m.Ma })
                .ToListAsync())
            .ToLookup(x => x.MonHocId);

        var siSo = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => lopIds.Contains(x.LopHocPhanId) && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .GroupBy(x => x.LopHocPhanId)
            .Select(g => new { LopId = g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(x => x.LopId, x => x.SoLuong);

        var lopCanLich = lopIds.Concat(daCo.Select(x => x.LopHocPhanId)).Distinct().ToList();
        var lich = (await _db.Set<LichHoc>().AsNoTracking()
                .Where(x => lopCanLich.Contains(x.LopHocPhanId))
                .Select(x => new Buoi(x.LopHocPhanId, x.Thu, x.TietBatDau, x.SoTiet))
                .ToListAsync())
            .ToLookup(x => x.LopHocPhanId);

        foreach (var lop in cacLop)
        {
            var lyDo = ketQua[lop.Id].LyDo;

            // Loại đăng ký theo các lượt học môn này ở học kỳ trước: chưa học -> học mới,
            // đã có lượt đạt -> học cải thiện, còn lại (không đạt hoặc chưa có điểm) -> học lại
            var luotTruoc = daCo.Where(x => x.MonHocId == lop.MonHocId && x.NgayBatDauHocKy < ngayBatDauHocKy[lop.HocKyId]).ToList();
            ketQua[lop.Id].LoaiDangKy = luotTruoc.Count == 0 ? LoaiDangKy.HocMoi
                : luotTruoc.Any(x => x.DaDat) ? LoaiDangKy.HocCaiThien
                : LoaiDangKy.HocLai;

            if (sinhVien.TrangThai != TrangThaiSinhVien.DangHoc)
                lyDo.Add("Sinh viên không ở trạng thái đang học nên không được đăng ký học phần.");
            if (lop.TrangThai != TrangThaiLopHocPhan.MoDangKy)
                lyDo.Add("Lớp học phần không còn mở đăng ký.");
            if (!hocKyDangMo.Contains(lop.HocKyId))
                lyDo.Add("Học kỳ này chưa có đợt đăng ký đang mở hoặc đã ngoài thời gian đăng ký.");

            if (daCo.Any(x => x.LopHocPhanId == lop.Id))
            {
                lyDo.Add("Sinh viên đã đăng ký lớp học phần này.");
                continue; // Các điều kiện còn lại không còn ý nghĩa
            }

            var cungMon = daCo.FirstOrDefault(x => x.MonHocId == lop.MonHocId && x.HocKyId == lop.HocKyId);
            if (cungMon is not null)
                lyDo.Add($"Đã đăng ký môn {cungMon.MaMon} ở lớp {cungMon.MaLop} trong học kỳ này.");

            var daDangKy = siSo.GetValueOrDefault(lop.Id);
            if (daDangKy >= lop.SiSoToiDa)
                lyDo.Add($"Lớp học phần đã đủ sĩ số ({daDangKy}/{lop.SiSoToiDa}).");

            var thieu = tienQuyet[lop.MonHocId].Where(x => !monDaDat.Contains(x.TienQuyetId)).Select(x => x.Ma).ToList();
            if (thieu.Count > 0)
                lyDo.Add($"Chưa hoàn thành môn tiên quyết: {string.Join(", ", thieu)}.");

            var lopCungKy = daCo.Where(x => x.HocKyId == lop.HocKyId).ToList();
            foreach (var buoi in lich[lop.Id])
            {
                foreach (var khac in lopCungKy)
                {
                    var trung = lich[khac.LopHocPhanId].FirstOrDefault(b => b.Giao(buoi));
                    if (trung is not null)
                        lyDo.Add($"Trùng lịch với lớp {khac.MaLop} (thứ {trung.Thu}, tiết {trung.TietBatDau}-{trung.TietKetThuc}).");
                }
            }

            var tongTinChi = lopCungKy.Sum(x => x.SoTinChi) + tinChiMon.GetValueOrDefault(lop.MonHocId);
            if (tongTinChi > tinChiToiDa)
                lyDo.Add($"Vượt số tín chỉ tối đa mỗi học kỳ ({tongTinChi}/{tinChiToiDa}).");

            // Một lớp có nhiều buổi cùng trùng một lớp khác sẽ sinh lý do lặp
            var khongLap = lyDo.Distinct().ToList();
            lyDo.Clear();
            lyDo.AddRange(khongLap);
        }

        return ketQua;
    }
}
