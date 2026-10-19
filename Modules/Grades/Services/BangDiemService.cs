using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>
/// Cách tính:
/// - GPA học kỳ: trung bình điểm hệ 4 theo tín chỉ của các môn ĐÃ KHÓA ĐIỂM trong học kỳ đó.
///   Nếu học kỳ còn môn chưa khóa điểm thì GPA được đánh dấu "tạm tính".
/// - CPA: tính đến hết từng học kỳ; môn học nhiều lần chỉ lấy lần có điểm hệ 4 cao nhất.
/// </summary>
public class BangDiemService(AppDbContext db) : IBangDiemService
{
    private readonly AppDbContext _db = db;

    public async Task<BangDiemSinhVienDto> GetBangDiemAsync(int sinhVienId)
    {
        var bangDiem = await GetBangDiemNhieuSinhVienAsync([sinhVienId]);
        return bangDiem.GetValueOrDefault(sinhVienId) ?? throw AppException.NotFound("sinh viên");
    }

    public async Task<Dictionary<int, BangDiemSinhVienDto>> GetBangDiemNhieuSinhVienAsync(int[] sinhVienIds)
    {
        var sinhVien = await _db.Set<SinhVien>().AsNoTracking()
            .Where(x => sinhVienIds.Contains(x.Id))
            .Select(x => new { x.Id, x.MSSV, x.HoTen })
            .ToListAsync();
        if (sinhVien.Count == 0) return new();

        var luotHoc = await (
            from dangKy in _db.Set<DangKyHocPhan>().AsNoTracking()
            where sinhVienIds.Contains(dangKy.SinhVienId) && dangKy.TrangThai == TrangThaiDangKy.DaDangKy
            join lop in _db.Set<LopHocPhan>().AsNoTracking() on dangKy.LopHocPhanId equals lop.Id
            join mon in _db.Set<MonHoc>().AsNoTracking() on lop.MonHocId equals mon.Id
            join hocKy in _db.Set<HocKy>().AsNoTracking() on lop.HocKyId equals hocKy.Id
            join namHoc in _db.Set<NamHoc>().AsNoTracking() on hocKy.NamHocId equals namHoc.Id
            join diem in _db.Set<DiemHocPhan>().AsNoTracking()
                on dangKy.Id equals diem.DangKyHocPhanId into nhomDiem
            from diem in nhomDiem.DefaultIfEmpty()
            select new LuotHoc(dangKy.SinhVienId, dangKy.Id, diem == null ? null : diem.Id, dangKy.LoaiDangKy,
                mon.Id, mon.Ma, mon.Ten, mon.SoTinChi, hocKy.Id, hocKy.Ten, hocKy.SoThuTu,
                hocKy.NgayBatDau, namHoc.Ten, namHoc.NgayBatDau,
                diem == null ? null : diem.DiemTongKet,
                diem == null ? null : diem.DiemChu,
                diem != null && diem.DaKhoa)).ToListAsync();

        var luotTheoSinhVien = luotHoc.ToLookup(x => x.SinhVienId);
        return sinhVien.ToDictionary(x => x.Id,
            x => TinhBangDiem(x.Id, x.MSSV, x.HoTen, luotTheoSinhVien[x.Id]));
    }

    /// <summary>Một lượt học của sinh viên, đã kèm thông tin môn, học kỳ và điểm.</summary>
    private sealed record LuotHoc(int SinhVienId, int DangKyHocPhanId, int? DiemHocPhanId, LoaiDangKy LoaiDangKy,
        int MonHocId, string MaMonHoc, string TenMonHoc, int SoTinChi, int HocKyId,
        string TenHocKy, int SoThuTu, DateOnly NgayBatDauHocKy, string TenNamHoc,
        DateOnly NgayBatDauNamHoc, decimal? DiemTongKet, string? DiemChu, bool DaKhoa)
    {
        /// <summary>Chỉ điểm đã khóa và đã tính tổng kết mới được dùng để tính GPA/CPA.</summary>
        public bool CoKetQuaKhoa => DaKhoa && DiemTongKet is not null && !string.IsNullOrWhiteSpace(DiemChu);
    }

    private static BangDiemSinhVienDto TinhBangDiem(
        int sinhVienId, string mssv, string hoTen, IEnumerable<LuotHoc> luotHoc)
    {
        // Kết quả tốt nhất của từng môn (theo MonHocId) tính đến học kỳ đang xét, dùng cho CPA
        var ketQuaCaoNhat = new Dictionary<int, (decimal DiemHe4, int SoTinChi, bool Dat)>();
        var bangDiemHocKy = new List<BangDiemHocKyDto>();
        var cacHocKy = luotHoc.GroupBy(x => new
            {
                x.HocKyId, x.TenHocKy, x.TenNamHoc, x.NgayBatDauNamHoc,
                x.SoThuTu, x.NgayBatDauHocKy
            })
            .OrderBy(x => x.Key.NgayBatDauNamHoc)
            .ThenBy(x => x.Key.SoThuTu)
            .ThenBy(x => x.Key.NgayBatDauHocKy)
            .ThenBy(x => x.Key.HocKyId);

        foreach (var hocKy in cacHocKy)
        {
            var monHoc = new List<BangDiemMonHocDto>();
            var tongDiemGpa = 0m;
            var tinChiGpa = 0;
            var soChuaCoKetQuaKhoa = 0;

            foreach (var luot in hocKy.OrderBy(x => x.DangKyHocPhanId))
            {
                decimal? diemHe4 = null;
                if (luot.CoKetQuaKhoa)
                {
                    if (luot.SoTinChi < 0)
                        throw AppException.Conflict($"Môn học {luot.MaMonHoc} có số tín chỉ không hợp lệ.");
                    diemHe4 = GradeCalculator.QuyDoiDiemHe4(luot.DiemChu!);
                    tongDiemGpa += diemHe4.Value * luot.SoTinChi;
                    tinChiGpa += luot.SoTinChi;

                    // Môn học lại chỉ thay kết quả CPA nếu điểm hệ 4 cao hơn.
                    if (!ketQuaCaoNhat.TryGetValue(luot.MonHocId, out var cu) || diemHe4.Value > cu.DiemHe4)
                        ketQuaCaoNhat[luot.MonHocId] =
                            (diemHe4.Value, luot.SoTinChi, GradeCalculator.LaDiemDat(luot.DiemTongKet!.Value));
                }
                else
                {
                    soChuaCoKetQuaKhoa++;
                }

                monHoc.Add(new BangDiemMonHocDto(
                    luot.DangKyHocPhanId, luot.DiemHocPhanId, luot.MonHocId, luot.MaMonHoc, luot.TenMonHoc,
                    luot.SoTinChi, luot.LoaiDangKy.ToString(), luot.DiemTongKet,
                    luot.DiemChu, diemHe4, luot.DaKhoa));
            }

            var tinChiCpa = ketQuaCaoNhat.Values.Sum(x => x.SoTinChi);
            var tongDiemCpa = ketQuaCaoNhat.Values.Sum(x => x.DiemHe4 * x.SoTinChi);
            bangDiemHocKy.Add(new BangDiemHocKyDto(
                hocKy.Key.HocKyId, hocKy.Key.TenHocKy, hocKy.Key.TenNamHoc,
                GradeCalculator.TinhTrungBinh(tongDiemGpa, tinChiGpa),
                GradeCalculator.TinhTrungBinh(tongDiemCpa, tinChiCpa),
                tinChiGpa, soChuaCoKetQuaKhoa, soChuaCoKetQuaKhoa > 0, monHoc));
        }

        return new BangDiemSinhVienDto(
            sinhVienId, mssv, hoTen, bangDiemHocKy.LastOrDefault()?.Cpa,
            ketQuaCaoNhat.Values.Sum(x => x.SoTinChi),
            ketQuaCaoNhat.Values.Where(x => x.Dat).Sum(x => x.SoTinChi), bangDiemHocKy);
    }
}
