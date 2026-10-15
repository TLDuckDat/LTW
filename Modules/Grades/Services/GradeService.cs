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

public class GradeService(AppDbContext db) : IGradeService
{
    private readonly AppDbContext _db = db;

    // GRADE-01 Danh sách sinh viên của lớp học phần
    public async Task<PagedResult<SinhVienTrongLopDto>?> GetDanhSachSinhVienAsync(int lopHocPhanId, PagedQuery query)
    {
        if (!await _db.Set<LopHocPhan>().AnyAsync(x => x.Id == lopHocPhanId)) return null;

        var dangKy = _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .Join(_db.Set<SinhVien>().AsNoTracking(), dk => dk.SinhVienId, sv => sv.Id, (dk, sv) => new { dk, sv });

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            dangKy = dangKy.Where(x => x.sv.MSSV.Contains(search) || x.sv.HoTen.Contains(search));
        }

        var total = await dangKy.CountAsync();
        var items = await dangKy.OrderBy(x => x.sv.MSSV)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new SinhVienTrongLopDto(x.dk.Id, x.sv.MSSV, x.sv.HoTen, x.dk.LoaiDangKy.ToString()))
            .ToListAsync();

        return new PagedResult<SinhVienTrongLopDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    // GRADE-01 Thêm sinh viên vào lớp học phần
    public async Task<DangKyHocPhanDto?> ThemSinhVienAsync(int lopHocPhanId, ThemSinhVienRequest request)
    {
        var lop = await GetLopAsync(lopHocPhanId);
        var sinhVienTonTai = await _db.Set<SinhVien>().AnyAsync(x => x.Id == request.SinhVienId);
        if (lop is null || !sinhVienTonTai) return null;

        if (lop.TrangThai != TrangThaiLopHocPhan.MoDangKy)
            throw new InvalidOperationException("Lớp học phần không còn cho phép đăng ký.");

        var dangKy = await _db.Set<DangKyHocPhan>()
            .FirstOrDefaultAsync(x => x.SinhVienId == request.SinhVienId && x.LopHocPhanId == lopHocPhanId);
        if (dangKy?.TrangThai == TrangThaiDangKy.DaDangKy)
            throw new InvalidOperationException("Sinh viên đã có trong lớp học phần này.");
        if (await DaDangKyMonTrongHocKyAsync(request.SinhVienId, lop))
            throw new InvalidOperationException("Sinh viên đã đăng ký môn học này ở lớp khác trong cùng học kỳ.");

        if (await LopDaDuSiSoAsync(lop))
            throw new InvalidOperationException("Lớp học phần đã đủ sĩ số.");

        var loaiDangKy = await XacDinhLoaiDangKyAsync(request.SinhVienId, lop);
        if (dangKy is null)
        {
            dangKy = new DangKyHocPhan
            {
                SinhVienId = request.SinhVienId, LopHocPhanId = lopHocPhanId, LoaiDangKy = loaiDangKy
            };
            _db.Add(dangKy);
        }
        else
        {
            // Lượt đăng ký cũ đã hủy: dùng lại vì cặp (SinhVienId, LopHocPhanId) là unique
            if (await CoDiemAsync(dangKy.Id))
                throw new InvalidOperationException("Lượt đăng ký cũ đã có điểm, không thể đăng ký lại.");
            dangKy.TrangThai = TrangThaiDangKy.DaDangKy;
            dangKy.LoaiDangKy = loaiDangKy;
        }

        await _db.SaveChangesAsync();
        return ToDto(dangKy);
    }

    // GRADE-01 Chuyển lượt đăng ký sang lớp học phần khác cùng môn
    public async Task<DangKyHocPhanDto?> ChuyenLopAsync(int lopHocPhanId, int dangKyHocPhanId, ChuyenLopRequest request)
    {
        var lopCu = await GetLopAsync(lopHocPhanId);
        var lopMoi = await GetLopAsync(request.LopHocPhanMoiId);
        var dangKy = await _db.Set<DangKyHocPhan>()
            .FirstOrDefaultAsync(x => x.Id == dangKyHocPhanId && x.LopHocPhanId == lopHocPhanId);
        if (lopCu is null || lopMoi is null || dangKy is null) return null;

        KiemTraLopMoDangKy(lopCu);
        if (dangKy.TrangThai != TrangThaiDangKy.DaDangKy)
            throw new InvalidOperationException("Lượt đăng ký đã bị hủy, không thể chuyển lớp.");
        if (lopMoi.TrangThai != TrangThaiLopHocPhan.MoDangKy)
            throw new InvalidOperationException("Lớp học phần mới không còn nhận đăng ký.");
        if (lopMoi.MonHocId != lopCu.MonHocId)
            throw new InvalidOperationException("Lớp học phần mới không cùng môn học.");
        // Đổi học kỳ sẽ làm sai dữ liệu tính GPA, nên chỉ cho chuyển trong cùng học kỳ
        if (lopMoi.HocKyId != lopCu.HocKyId)
            throw new InvalidOperationException("Lớp học phần mới không cùng học kỳ.");
        if (await CoDiemAsync(dangKy.Id))
            throw new InvalidOperationException("Lượt đăng ký đã có điểm, không thể chuyển lớp.");

        // Lượt đăng ký ở lớp mới (kể cả đã hủy) đều làm trùng cặp unique, nên chặn luôn
        var daCoLopMoi = await _db.Set<DangKyHocPhan>()
            .AnyAsync(x => x.SinhVienId == dangKy.SinhVienId && x.LopHocPhanId == lopMoi.Id);
        if (daCoLopMoi)
            throw new InvalidOperationException("Sinh viên đã có lượt đăng ký ở lớp học phần mới.");
        // Bỏ qua chính lượt đang chuyển, chỉ chặn khi còn lượt khác cùng môn cùng học kỳ
        if (await DaDangKyMonTrongHocKyAsync(dangKy.SinhVienId, lopMoi, dangKy.Id))
            throw new InvalidOperationException("Sinh viên đã đăng ký môn học này ở lớp khác trong cùng học kỳ.");

        if (await LopDaDuSiSoAsync(lopMoi))
            throw new InvalidOperationException("Lớp học phần mới đã đủ sĩ số.");

        dangKy.LopHocPhanId = lopMoi.Id;
        await _db.SaveChangesAsync();
        return ToDto(dangKy);
    }

    // GRADE-01 Hủy lượt đăng ký, chỉ đổi trạng thái để giữ lịch sử
    public async Task<bool> HuyDangKyAsync(int lopHocPhanId, int dangKyHocPhanId)
    {
        var lop = await GetLopAsync(lopHocPhanId);
        var dangKy = await _db.Set<DangKyHocPhan>()
            .FirstOrDefaultAsync(x => x.Id == dangKyHocPhanId && x.LopHocPhanId == lopHocPhanId);
        if (lop is null || dangKy is null) return false;

        KiemTraLopMoDangKy(lop);
        if (dangKy.TrangThai == TrangThaiDangKy.DaHuy)
            throw new InvalidOperationException("Lượt đăng ký này đã được hủy trước đó.");
        if (await CoDiemAsync(dangKy.Id))
            throw new InvalidOperationException("Lượt đăng ký đã có điểm, không thể hủy.");

        dangKy.TrangThai = TrangThaiDangKy.DaHuy;
        await _db.SaveChangesAsync();
        return true;
    }

    // GRADE-02 Nhập và cập nhật điểm theo lớp học phần
    public async Task<NhapDiemResultDto?> NhapDiemAsync(int lopHocPhanId, NhapDiemRequest request)
    {
        var lop = await GetLopAsync(lopHocPhanId);
        if (lop is null) return null;
        KiemTraLopDuocNhapDiem(lop);

        // DataAnnotations bỏ qua phần tử null trong danh sách, nên phải kiểm tra ở đây
        if (request.DanhSachDiem.Any(x => x is null))
            throw new ArgumentException("Danh sách điểm có phần tử rỗng.");

        var dangKyIds = request.DanhSachDiem.Select(x => x.DangKyHocPhanId).ToList();
        if (dangKyIds.Distinct().Count() != dangKyIds.Count)
            throw new ArgumentException("Danh sách có mã đăng ký trùng nhau.");

        var dangKyTrongLop = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.LopHocPhanId == lopHocPhanId && dangKyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);
        var diemHienCo = await _db.Set<DiemHocPhan>()
            .Where(x => dangKyIds.Contains(x.DangKyHocPhanId))
            .ToDictionaryAsync(x => x.DangKyHocPhanId);

        // Kiểm tra toàn bộ danh sách trước, chỉ lưu khi mọi phần tử đều hợp lệ
        foreach (var item in request.DanhSachDiem)
        {
            if (!dangKyTrongLop.TryGetValue(item.DangKyHocPhanId, out var dangKy))
                throw new ArgumentException($"Lượt đăng ký {item.DangKyHocPhanId} không thuộc lớp học phần này.");
            if (dangKy.TrangThai != TrangThaiDangKy.DaDangKy)
                throw new ArgumentException($"Lượt đăng ký {item.DangKyHocPhanId} đã bị hủy.");
            if (CoHonHaiChuSoThapPhan(item))
                throw new ArgumentException($"Điểm của lượt đăng ký {item.DangKyHocPhanId} chỉ được có tối đa 2 chữ số thập phân.");

            if (diemHienCo.TryGetValue(item.DangKyHocPhanId, out var diemCu))
            {
                if (diemCu.DaKhoa)
                    throw new InvalidOperationException($"Điểm của lượt đăng ký {item.DangKyHocPhanId} đã khóa, không thể cập nhật.");
            }
            else if (item.DiemQuaTrinh is null && item.DiemGiuaKy is null && item.DiemCuoiKy is null)
            {
                throw new ArgumentException($"Lượt đăng ký {item.DangKyHocPhanId} chưa có điểm nào để nhập.");
            }
        }

        var soThemMoi = 0;
        var soCapNhat = 0;
        var soKhongThayDoi = 0;
        foreach (var item in request.DanhSachDiem)
        {
            if (!diemHienCo.TryGetValue(item.DangKyHocPhanId, out var diem))
            {
                _db.Add(new DiemHocPhan
                {
                    DangKyHocPhanId = item.DangKyHocPhanId,
                    DiemQuaTrinh = item.DiemQuaTrinh,
                    DiemGiuaKy = item.DiemGiuaKy,
                    DiemCuoiKy = item.DiemCuoiKy
                });
                soThemMoi++;
            }
            else if (diem.DiemQuaTrinh == item.DiemQuaTrinh && diem.DiemGiuaKy == item.DiemGiuaKy && diem.DiemCuoiKy == item.DiemCuoiKy)
            {
                soKhongThayDoi++;
            }
            else
            {
                // Đổi điểm thành phần thì điểm tổng kết, điểm chữ cũ không còn đúng: xóa để GRADE-03 tính lại
                diem.DiemQuaTrinh = item.DiemQuaTrinh;
                diem.DiemGiuaKy = item.DiemGiuaKy;
                diem.DiemCuoiKy = item.DiemCuoiKy;
                diem.DiemTongKet = null;
                diem.DiemChu = null;
                soCapNhat++;
            }
        }

        await _db.SaveChangesAsync();
        return new NhapDiemResultDto(lopHocPhanId, request.DanhSachDiem.Count, soThemMoi, soCapNhat, soKhongThayDoi);
    }

    // GRADE-03 Tính điểm tổng kết, điểm chữ
    public async Task<TinhDiemResultDto?> TinhDiemAsync(int lopHocPhanId)
    {
        var lop = await GetLopAsync(lopHocPhanId);
        if (lop is null) return null;
        if (lop.TrangThai is not (TrangThaiLopHocPhan.DangHoc or TrangThaiLopHocPhan.DaKetThuc))
            throw new InvalidOperationException("Lớp học phần chưa bắt đầu hoặc đã bị hủy, không thể tính điểm.");

        var danhSachDiem = await _db.Set<DiemHocPhan>()
            .Join(_db.Set<DangKyHocPhan>()
                    .Where(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy),
                diem => diem.DangKyHocPhanId,
                dangKy => dangKy.Id,
                (diem, dangKy) => diem)
            .ToListAsync();

        var soDaTinh = 0;
        var soThieuDiem = 0;
        var soDaKhoa = 0;
        var soKhongThayDoi = 0;

        foreach (var diem in danhSachDiem)
        {
            if (diem.DaKhoa)
            {
                soDaKhoa++;
                continue;
            }

            if (diem.DiemQuaTrinh is null || diem.DiemGiuaKy is null || diem.DiemCuoiKy is null)
            {
                soThieuDiem++;
                continue;
            }

            var tongKet = TinhDiemTongKet(
                diem.DiemQuaTrinh.Value, diem.DiemGiuaKy.Value, diem.DiemCuoiKy.Value);
            var diemChu = QuyDoiDiemChu(tongKet);

            if (diem.DiemTongKet == tongKet && diem.DiemChu == diemChu)
            {
                soKhongThayDoi++;
                continue;
            }

            diem.DiemTongKet = tongKet;
            diem.DiemChu = diemChu;
            soDaTinh++;
        }

        if (soDaTinh > 0) await _db.SaveChangesAsync();

        return new TinhDiemResultDto(
            lopHocPhanId, danhSachDiem.Count, soDaTinh, soThieuDiem, soDaKhoa, soKhongThayDoi);
    }

    // GRADE-04 Duyệt và khóa bảng điểm
    public async Task<KhoaDiemResultDto?> KhoaDiemAsync(int lopHocPhanId)
    {
        var lop = await GetLopAsync(lopHocPhanId);
        if (lop is null) return null;
        if (lop.TrangThai != TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Chỉ được duyệt và khóa bảng điểm khi lớp học phần đã kết thúc.");

        var dangKyIds = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .Select(x => x.Id)
            .ToListAsync();
        if (dangKyIds.Count == 0)
            throw new InvalidOperationException("Lớp học phần không có sinh viên đang đăng ký để duyệt bảng điểm.");

        var diemTheoDangKy = await _db.Set<DiemHocPhan>()
            .Where(x => dangKyIds.Contains(x.DangKyHocPhanId))
            .ToDictionaryAsync(x => x.DangKyHocPhanId);

        // Kiểm tra từ danh sách đăng ký để không bỏ sót sinh viên chưa có bản ghi điểm.
        var soThieuDiem = dangKyIds.Count(id =>
            !diemTheoDangKy.TryGetValue(id, out var diem) ||
            diem.DiemQuaTrinh is null || diem.DiemGiuaKy is null || diem.DiemCuoiKy is null ||
            diem.DiemTongKet is null || string.IsNullOrWhiteSpace(diem.DiemChu));
        if (soThieuDiem > 0)
            throw new InvalidOperationException($"Có {soThieuDiem} sinh viên chưa đủ điểm để duyệt và khóa bảng điểm.");

        var soDaKhoaTruocDo = 0;
        foreach (var diem in diemTheoDangKy.Values)
        {
            if (diem.DaKhoa) soDaKhoaTruocDo++;
            else diem.DaKhoa = true;
        }

        var soMoiKhoa = dangKyIds.Count - soDaKhoaTruocDo;
        if (soMoiKhoa > 0) await _db.SaveChangesAsync();

        return new KhoaDiemResultDto(lopHocPhanId, dangKyIds.Count, soMoiKhoa, soDaKhoaTruocDo);
    }

    // GRADE-06 Bảng điểm sinh viên, GPA học kỳ và CPA tích lũy
    public async Task<BangDiemSinhVienDto?> GetBangDiemAsync(int sinhVienId)
    {
        var bangDiem = await GetBangDiemNhieuSinhVienAsync([sinhVienId]);
        return bangDiem.GetValueOrDefault(sinhVienId);
    }

    private sealed record LuotHocBangDiem(int SinhVienId, int DangKyHocPhanId, LoaiDangKy LoaiDangKy,
        int MonHocId, string MaMonHoc, string TenMonHoc, int SoTinChi, int HocKyId,
        string TenHocKy, int SoThuTu, DateOnly NgayBatDauHocKy, string TenNamHoc,
        DateOnly NgayBatDauNamHoc, decimal? DiemTongKet, string? DiemChu, bool DaKhoa);

    // Cùng phép tính GRADE-06 cho một hoặc nhiều sinh viên; danh sách cảnh báo chỉ cần hai truy vấn đọc.
    private async Task<Dictionary<int, BangDiemSinhVienDto>> GetBangDiemNhieuSinhVienAsync(int[] sinhVienIds)
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
            select new LuotHocBangDiem(dangKy.SinhVienId, dangKy.Id, dangKy.LoaiDangKy,
                mon.Id, mon.Ma, mon.Ten, mon.SoTinChi, hocKy.Id, hocKy.Ten, hocKy.SoThuTu,
                hocKy.NgayBatDau, namHoc.Ten, namHoc.NgayBatDau,
                diem == null ? null : diem.DiemTongKet,
                diem == null ? null : diem.DiemChu,
                diem != null && diem.DaKhoa)).ToListAsync();

        var luotTheoSinhVien = luotHoc.ToLookup(x => x.SinhVienId);
        return sinhVien.ToDictionary(x => x.Id,
            x => TinhBangDiemSinhVien(x.Id, x.MSSV, x.HoTen, luotTheoSinhVien[x.Id]));
    }

    private static BangDiemSinhVienDto TinhBangDiemSinhVien(
        int sinhVienId, string mssv, string hoTen, IEnumerable<LuotHocBangDiem> luotHoc)
    {
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
                var coKetQuaKhoa = luot.DaKhoa && luot.DiemTongKet is not null &&
                    !string.IsNullOrWhiteSpace(luot.DiemChu);
                decimal? diemHe4 = null;
                if (coKetQuaKhoa)
                {
                    if (luot.SoTinChi < 0)
                        throw new InvalidOperationException($"Môn học {luot.MaMonHoc} có số tín chỉ không hợp lệ.");
                    diemHe4 = QuyDoiDiemHe4(luot.DiemChu!);
                    tongDiemGpa += diemHe4.Value * luot.SoTinChi;
                    tinChiGpa += luot.SoTinChi;

                    // Môn học lại chỉ thay kết quả CPA nếu điểm hệ 4 cao hơn.
                    if (!ketQuaCaoNhat.TryGetValue(luot.MonHocId, out var cu) || diemHe4.Value > cu.DiemHe4)
                        ketQuaCaoNhat[luot.MonHocId] =
                            (diemHe4.Value, luot.SoTinChi, luot.DiemTongKet!.Value >= 4m);
                }
                else
                {
                    soChuaCoKetQuaKhoa++;
                }

                monHoc.Add(new BangDiemMonHocDto(
                    luot.DangKyHocPhanId, luot.MonHocId, luot.MaMonHoc, luot.TenMonHoc,
                    luot.SoTinChi, luot.LoaiDangKy.ToString(), luot.DiemTongKet,
                    luot.DiemChu, diemHe4, luot.DaKhoa));
            }

            var tinChiCpa = ketQuaCaoNhat.Values.Sum(x => x.SoTinChi);
            var tongDiemCpa = ketQuaCaoNhat.Values.Sum(x => x.DiemHe4 * x.SoTinChi);
            bangDiemHocKy.Add(new BangDiemHocKyDto(
                hocKy.Key.HocKyId, hocKy.Key.TenHocKy, hocKy.Key.TenNamHoc,
                TinhTrungBinh(tongDiemGpa, tinChiGpa), TinhTrungBinh(tongDiemCpa, tinChiCpa),
                tinChiGpa, soChuaCoKetQuaKhoa, soChuaCoKetQuaKhoa > 0, monHoc));
        }

        return new BangDiemSinhVienDto(
            sinhVienId, mssv, hoTen, bangDiemHocKy.LastOrDefault()?.Cpa,
            ketQuaCaoNhat.Values.Sum(x => x.SoTinChi),
            ketQuaCaoNhat.Values.Where(x => x.Dat).Sum(x => x.SoTinChi), bangDiemHocKy);
    }

    // GRADE-07 Đánh giá lại mỗi lần đọc để kết quả phúc khảo được phản ánh ngay.
    public async Task<CanhBaoHocVuDto?> GetCanhBaoHocVuAsync(int sinhVienId, int hocKyId)
    {
        var hocKy = await GetHocKyCanhBaoAsync(hocKyId);
        if (hocKy is null) return null;
        var sinhVien = await GetSinhVienCanhBaoQuery(sinhVienId: sinhVienId).FirstOrDefaultAsync();
        if (sinhVien is null) return null;
        var bangDiem = await GetBangDiemAsync(sinhVienId);
        return DanhGiaCanhBao(sinhVien, hocKy, bangDiem!);
    }

    public async Task<PagedResult<CanhBaoHocVuDto>?> GetCanhBaoHocVuPagedAsync(int hocKyId, PagedQuery query)
    {
        var hocKy = await GetHocKyCanhBaoAsync(hocKyId);
        if (hocKy is null) return null;

        // Chỉ lấy sinh viên có lượt đăng ký còn hiệu lực ở học kỳ cần xét.
        var sinhVienIds = _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.TrangThai == TrangThaiDangKy.DaDangKy)
            .Join(_db.Set<LopHocPhan>().AsNoTracking().Where(x => x.HocKyId == hocKyId),
                dk => dk.LopHocPhanId, lop => lop.Id, (dk, lop) => dk.SinhVienId)
            .Distinct();
        var sinhVien = await GetSinhVienCanhBaoQuery(sinhVienIds: sinhVienIds).ToListAsync();

        var bangDiemTheoSinhVien = await GetBangDiemNhieuSinhVienAsync(sinhVien.Select(x => x.Id).ToArray());
        var canhBao = new List<CanhBaoHocVuDto>();
        foreach (var sv in sinhVien)
        {
            var ketQua = DanhGiaCanhBao(sv, hocKy, bangDiemTheoSinhVien[sv.Id]);
            if (ketQua.TrangThaiCanhBao == "CanhBao") canhBao.Add(ketQua);
        }

        return new PagedResult<CanhBaoHocVuDto>
        {
            Items = canhBao.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = canhBao.Count
        };
    }

    private sealed record SinhVienCanhBao(int Id, string MSSV, string HoTen, int? KhoaTuyen);
    private sealed record HocKyCanhBao(int Id, string Ten, int SoThuTu, DateOnly NgayKetThuc,
        string TenNamHoc, int NamBatDau);

    private IQueryable<SinhVienCanhBao> GetSinhVienCanhBaoQuery(
        int? sinhVienId = null, IQueryable<int>? sinhVienIds = null)
    {
        var query = _db.Set<SinhVien>().AsNoTracking().AsQueryable();
        if (sinhVienId is not null) query = query.Where(x => x.Id == sinhVienId);
        if (sinhVienIds is not null) query = query.Where(x => sinhVienIds.Contains(x.Id));
        return from sv in query.OrderBy(x => x.MSSV).ThenBy(x => x.Id)
               join lop in _db.Set<LopHanhChinh>().AsNoTracking()
                   on sv.LopHanhChinhId equals (int?)lop.Id into nhomLop
               from lop in nhomLop.DefaultIfEmpty()
               select new SinhVienCanhBao(sv.Id, sv.MSSV, sv.HoTen,
                   lop == null ? null : lop.KhoaTuyen);
    }

    private Task<HocKyCanhBao?> GetHocKyCanhBaoAsync(int hocKyId) =>
        (from hk in _db.Set<HocKy>().AsNoTracking()
         where hk.Id == hocKyId
         join nh in _db.Set<NamHoc>().AsNoTracking() on hk.NamHocId equals nh.Id
         select new HocKyCanhBao(hk.Id, hk.Ten, hk.SoThuTu, hk.NgayKetThuc,
             nh.Ten, nh.NgayBatDau.Year)).FirstOrDefaultAsync();

    private static CanhBaoHocVuDto DanhGiaCanhBao(
        SinhVienCanhBao sv, HocKyCanhBao hocKy, BangDiemSinhVienDto bangDiem)
    {
        var diemHocKy = bangDiem.DanhSachHocKy.FirstOrDefault(x => x.HocKyId == hocKy.Id);
        var monHoc = diemHocKy?.DanhSachMonHoc ?? [];
        var tongTinChiDangKy = monHoc.Sum(x => x.SoTinChi);
        var monKhongDat = monHoc.Where(x => x.DaKhoa && x.DiemTongKet is not null &&
            x.DiemHe4 is not null && x.DiemChu is "F" or "F+").ToList();
        var lyDo = new List<string>();
        var duDuLieu = true;

        // Xét thứ tự 1/2 như hai kỳ chính; các thứ tự khác chưa đủ căn cứ để xét riêng.
        if (hocKy.SoThuTu is not (1 or 2))
        {
            lyDo.Add("Học kỳ này chưa được xác định là học kỳ chính để xét cảnh báo.");
            duDuLieu = false;
        }
        if (hocKy.NgayKetThuc >= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            lyDo.Add("Học kỳ chưa kết thúc; chưa thể kết luận cảnh báo chính thức.");
            duDuLieu = false;
        }
        if (monHoc.Count == 0)
        {
            lyDo.Add("Sinh viên chưa có đăng ký còn hiệu lực trong học kỳ.");
            duDuLieu = false;
        }
        if (diemHocKy?.Gpa is null)
        {
            lyDo.Add("Chưa có GPA học kỳ từ kết quả đã khóa.");
            duDuLieu = false;
        }
        if (diemHocKy?.GpaTamTinh == true || diemHocKy?.SoHocPhanChuaCoKetQuaKhoa > 0)
        {
            lyDo.Add("Học kỳ còn học phần chưa có điểm được khóa.");
            duDuLieu = false;
        }
        if (sv.KhoaTuyen is null or <= 0 || hocKy.NamBatDau < sv.KhoaTuyen)
        {
            lyDo.Add("Chưa xác định được khóa tuyển để áp dụng ngưỡng GPA.");
            duDuLieu = false;
        }

        if (duDuLieu)
        {
            var nguongGpa = hocKy.SoThuTu == 1 && hocKy.NamBatDau == sv.KhoaTuyen ? 0.80m : 1.00m;
            if (diemHocKy!.Gpa < nguongGpa)
                lyDo.Add($"GPA học kỳ {diemHocKy.Gpa:0.00} thấp hơn mức {nguongGpa:0.00}.");
            if (tongTinChiDangKy < 12)
                lyDo.Add($"Sinh viên chỉ đăng ký {tongTinChiDangKy} tín chỉ, thấp hơn mức tối thiểu 12 tín chỉ.");
        }

        var trangThai = !duDuLieu ? "ChuaDuDuLieu" : lyDo.Count > 0 ? "CanhBao" : "KhongCanhBao";
        return new CanhBaoHocVuDto(sv.Id, sv.MSSV, sv.HoTen, hocKy.Id, hocKy.Ten,
            hocKy.TenNamHoc, diemHocKy?.Gpa, diemHocKy?.Cpa, tongTinChiDangKy,
            monKhongDat.Count, monKhongDat.Sum(x => x.SoTinChi), trangThai, lyDo,
            "Đánh giá tự động; điều kiện tín chỉ cần Phòng đào tạo xác nhận các trường hợp được công nhận tín chỉ.");
    }

    // GRADE-05 Quản lý yêu cầu phúc khảo và điều chỉnh điểm
    public async Task<PhucKhaoDto?> CreatePhucKhaoAsync(PhucKhaoRequest request)
    {
        if (request.DiemHocPhanId <= 0)
            throw new ArgumentException("Mã bản ghi điểm phải lớn hơn 0.");
        var lyDo = request.LyDo?.Trim();
        if (string.IsNullOrWhiteSpace(lyDo) || lyDo.Length > 500)
            throw new ArgumentException("Lý do phúc khảo phải có từ 1 đến 500 ký tự.");

        var diem = await _db.Set<DiemHocPhan>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.DiemHocPhanId);
        if (diem is null) return null;

        var dangKy = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == diem.DangKyHocPhanId);
        var lop = dangKy is null ? null : await GetLopAsync(dangKy.LopHocPhanId);
        if (dangKy?.TrangThai != TrangThaiDangKy.DaDangKy || lop?.TrangThai != TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Chỉ phúc khảo điểm của lượt đăng ký còn hiệu lực trong lớp đã kết thúc.");
        if (!diem.DaKhoa || diem.DiemCuoiKy is null)
            throw new InvalidOperationException("Điểm cuối kỳ phải có giá trị và đã được duyệt, khóa trước khi phúc khảo.");
        if (await _db.Set<PhucKhao>().AnyAsync(x =>
                x.DiemHocPhanId == diem.Id && x.TrangThai == TrangThaiPhucKhao.ChoXuLy))
            throw new InvalidOperationException("Bản ghi điểm đã có yêu cầu phúc khảo đang chờ xử lý.");

        var phucKhao = new PhucKhao
        {
            DiemHocPhanId = diem.Id,
            LyDo = lyDo,
            DiemCu = diem.DiemCuoiKy,
            DiemMoi = null,
            TrangThai = TrangThaiPhucKhao.ChoXuLy
        };
        _db.Add(phucKhao);
        await _db.SaveChangesAsync();
        return await GetPhucKhaoByIdAsync(phucKhao.Id);
    }

    public async Task<PagedResult<PhucKhaoDto>> GetPhucKhaoPagedAsync(PagedQuery query, string? trangThai)
    {
        var phucKhao = _db.Set<PhucKhao>().AsNoTracking().AsQueryable();
        if (trangThai is not null)
        {
            var giaTriLoc = trangThai.Trim();
            if (!Enum.GetNames<TrangThaiPhucKhao>()
                    .Any(x => x.Equals(giaTriLoc, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Trạng thái phúc khảo không hợp lệ.");
            Enum.TryParse<TrangThaiPhucKhao>(giaTriLoc, true, out var trangThaiLoc);
            phucKhao = phucKhao.Where(x => x.TrangThai == trangThaiLoc);
        }

        var total = await phucKhao.CountAsync();
        var items = await ToPhucKhaoDtoQuery(phucKhao
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize))
            .ToListAsync();
        return new PagedResult<PhucKhaoDto>
        {
            Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total
        };
    }

    public Task<PhucKhaoDto?> GetPhucKhaoByIdAsync(int id) =>
        ToPhucKhaoDtoQuery(_db.Set<PhucKhao>().AsNoTracking().Where(x => x.Id == id))
            .FirstOrDefaultAsync();

    public async Task<PhucKhaoDto?> XuLyPhucKhaoAsync(int id, XuLyPhucKhaoRequest request)
    {
        var phucKhao = await _db.Set<PhucKhao>().FirstOrDefaultAsync(x => x.Id == id);
        if (phucKhao is null) return null;
        if (phucKhao.TrangThai != TrangThaiPhucKhao.ChoXuLy)
            throw new InvalidOperationException("Yêu cầu phúc khảo đã được xử lý.");
        if (request.ChapNhan is null)
            throw new ArgumentException("Cần chọn chấp nhận hoặc từ chối yêu cầu phúc khảo.");

        var diem = await _db.Set<DiemHocPhan>()
            .FirstOrDefaultAsync(x => x.Id == phucKhao.DiemHocPhanId);
        if (diem is null || !diem.DaKhoa)
            throw new InvalidOperationException("Bản ghi điểm không tồn tại hoặc không còn khóa.");

        var dangKy = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == diem.DangKyHocPhanId);
        var lop = dangKy is null ? null : await GetLopAsync(dangKy.LopHocPhanId);
        if (dangKy?.TrangThai != TrangThaiDangKy.DaDangKy || lop?.TrangThai != TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Lượt đăng ký hoặc lớp học phần không còn hợp lệ để xử lý phúc khảo.");

        if (request.ChapNhan.Value)
        {
            var diemMoi = request.DiemMoi;
            if (diemMoi is null || diemMoi < 0m || diemMoi > 10m ||
                decimal.Round(diemMoi.Value, 2) != diemMoi.Value)
                throw new ArgumentException("Điểm mới phải từ 0 đến 10 và có tối đa 2 chữ số thập phân.");
            if (diem.DiemCuoiKy is null || phucKhao.DiemCu != diem.DiemCuoiKy ||
                diem.DiemQuaTrinh is null || diem.DiemGiuaKy is null ||
                diem.DiemTongKet is null || string.IsNullOrWhiteSpace(diem.DiemChu))
                throw new InvalidOperationException("Dữ liệu điểm đã khóa không còn đầy đủ hoặc không khớp điểm cũ của yêu cầu.");
            if (diemMoi == diem.DiemCuoiKy)
                throw new InvalidOperationException("Điểm mới phải khác điểm cuối kỳ hiện tại.");

            diem.DiemCuoiKy = diemMoi;
            diem.DiemTongKet = TinhDiemTongKet(
                diem.DiemQuaTrinh.Value, diem.DiemGiuaKy.Value, diemMoi.Value);
            diem.DiemChu = QuyDoiDiemChu(diem.DiemTongKet.Value);
            phucKhao.DiemMoi = diemMoi;
            phucKhao.TrangThai = TrangThaiPhucKhao.DaChapNhan;
        }
        else
        {
            if (request.DiemMoi is not null)
                throw new ArgumentException("Không nhập điểm mới khi từ chối phúc khảo.");
            phucKhao.DiemMoi = null;
            phucKhao.TrangThai = TrangThaiPhucKhao.TuChoi;
        }

        await _db.SaveChangesAsync();
        return await GetPhucKhaoByIdAsync(phucKhao.Id);
    }

    private IQueryable<PhucKhaoDto> ToPhucKhaoDtoQuery(IQueryable<PhucKhao> phucKhao) =>
        from pk in phucKhao
        join diem in _db.Set<DiemHocPhan>().AsNoTracking() on pk.DiemHocPhanId equals diem.Id
        join dangKy in _db.Set<DangKyHocPhan>().AsNoTracking() on diem.DangKyHocPhanId equals dangKy.Id
        join sinhVien in _db.Set<SinhVien>().AsNoTracking() on dangKy.SinhVienId equals sinhVien.Id
        select new PhucKhaoDto(
            pk.Id, pk.DiemHocPhanId, sinhVien.Id, sinhVien.MSSV, sinhVien.HoTen,
            pk.LyDo, pk.DiemCu, pk.DiemMoi, pk.TrangThai.ToString(), pk.CreatedAt);

    private static decimal TinhDiemTongKet(decimal quaTrinh, decimal giuaKy, decimal cuoiKy) =>
        decimal.Round(quaTrinh * 0.15m + giuaKy * 0.15m + cuoiKy * 0.70m,
            1, MidpointRounding.AwayFromZero);

    private static decimal? TinhTrungBinh(decimal tongDiem, int soTinChi) => soTinChi == 0
        ? null
        : decimal.Round(tongDiem / soTinChi, 2, MidpointRounding.AwayFromZero);

    private static decimal QuyDoiDiemHe4(string diemChu) => diemChu switch
    {
        "A+" => 4.0m,
        "A" => 3.8m,
        "B+" => 3.5m,
        "B" => 3.0m,
        "C+" => 2.5m,
        "C" => 2.0m,
        "D+" => 1.5m,
        "D" => 1.0m,
        "F+" => 0.5m,
        "F" => 0.0m,
        _ => throw new InvalidOperationException($"Điểm chữ '{diemChu}' không hợp lệ.")
    };

    private static string QuyDoiDiemChu(decimal diemTongKet) => diemTongKet switch
    {
        >= 9.5m => "A+",
        >= 8.5m => "A",
        >= 8.0m => "B+",
        >= 7.0m => "B",
        >= 6.0m => "C+",
        >= 5.5m => "C",
        >= 4.5m => "D+",
        >= 4.0m => "D",
        >= 2.0m => "F+",
        _ => "F"
    };

    // Chuyển lớp và hủy đăng ký chỉ làm được khi lớp còn đang mở đăng ký
    private static void KiemTraLopMoDangKy(LopHocPhan lop)
    {
        if (lop.TrangThai == TrangThaiLopHocPhan.MoDangKy) return;

        var lyDo = lop.TrangThai switch
        {
            TrangThaiLopHocPhan.DangHoc => "đã bắt đầu học",
            TrangThaiLopHocPhan.DaKetThuc => "đã kết thúc",
            _ => "đã bị hủy"
        };
        throw new InvalidOperationException($"Lớp học phần {lyDo}, không thể chuyển hoặc hủy đăng ký.");
    }

    // Chỉ nhập điểm khi lớp đã bắt đầu học hoặc đã kết thúc
    private static void KiemTraLopDuocNhapDiem(LopHocPhan lop)
    {
        if (lop.TrangThai is TrangThaiLopHocPhan.DangHoc or TrangThaiLopHocPhan.DaKetThuc) return;

        var lyDo = lop.TrangThai == TrangThaiLopHocPhan.DaHuy ? "đã bị hủy" : "còn đang mở đăng ký";
        throw new InvalidOperationException($"Lớp học phần {lyDo}, chưa thể nhập điểm.");
    }

    // Điểm chỉ được tối đa 2 chữ số thập phân, ví dụ 7.5 hoặc 7.25 nhưng không nhận 7.555
    private static bool CoHonHaiChuSoThapPhan(DiemSinhVienRequest item) =>
        new[] { item.DiemQuaTrinh, item.DiemGiuaKy, item.DiemCuoiKy }
            .Any(diem => diem is not null && decimal.Round(diem.Value, 2) != diem.Value);

    private Task<LopHocPhan?> GetLopAsync(int lopHocPhanId) =>
        _db.Set<LopHocPhan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lopHocPhanId);

    // SiSoToiDa = 0 nghĩa là lớp không giới hạn sĩ số
    private async Task<bool> LopDaDuSiSoAsync(LopHocPhan lop)
    {
        if (lop.SiSoToiDa <= 0) return false;

        var siSoHienTai = await _db.Set<DangKyHocPhan>()
            .CountAsync(x => x.LopHocPhanId == lop.Id && x.TrangThai == TrangThaiDangKy.DaDangKy);
        return siSoHienTai >= lop.SiSoToiDa;
    }

    // Một sinh viên chỉ được có một lượt đăng ký còn hiệu lực cho mỗi môn trong một học kỳ
    private Task<bool> DaDangKyMonTrongHocKyAsync(int sinhVienId, LopHocPhan lop, int? boQuaDangKyId = null) =>
        (from dk in _db.Set<DangKyHocPhan>()
         join lhp in _db.Set<LopHocPhan>() on dk.LopHocPhanId equals lhp.Id
         where dk.SinhVienId == sinhVienId && dk.TrangThai == TrangThaiDangKy.DaDangKy
               && lhp.MonHocId == lop.MonHocId && lhp.HocKyId == lop.HocKyId
               && dk.Id != boQuaDangKyId
         select dk.Id).AnyAsync();

    // Xác định loại đăng ký theo các lượt học môn này ở những học kỳ trước:
    // chưa học -> HocMoi, đã có lượt đạt (điểm đã khóa >= 4) -> HocCaiThien, còn lại -> HocLai
    private async Task<LoaiDangKy> XacDinhLoaiDangKyAsync(int sinhVienId, LopHocPhan lop)
    {
        var ngayBatDauHocKy = await _db.Set<HocKy>()
            .Where(x => x.Id == lop.HocKyId)
            .Select(x => x.NgayBatDau)
            .FirstAsync();

        var ketQuaTruoc = await (
            from dk in _db.Set<DangKyHocPhan>()
            join lhp in _db.Set<LopHocPhan>() on dk.LopHocPhanId equals lhp.Id
            join hk in _db.Set<HocKy>() on lhp.HocKyId equals hk.Id
            where dk.SinhVienId == sinhVienId && dk.TrangThai == TrangThaiDangKy.DaDangKy
                  && lhp.MonHocId == lop.MonHocId && hk.NgayBatDau < ngayBatDauHocKy
            join diem in _db.Set<DiemHocPhan>() on dk.Id equals diem.DangKyHocPhanId into nhomDiem
            from diem in nhomDiem.DefaultIfEmpty()
            select diem != null && diem.DaKhoa && diem.DiemTongKet >= 4m).ToListAsync();

        if (ketQuaTruoc.Count == 0) return LoaiDangKy.HocMoi;
        return ketQuaTruoc.Any(dat => dat) ? LoaiDangKy.HocCaiThien : LoaiDangKy.HocLai;
    }

    private Task<bool> CoDiemAsync(int dangKyHocPhanId) =>
        _db.Set<DiemHocPhan>().AnyAsync(x => x.DangKyHocPhanId == dangKyHocPhanId);

    private static DangKyHocPhanDto ToDto(DangKyHocPhan x) =>
        new(x.Id, x.SinhVienId, x.LopHocPhanId, x.LoaiDangKy.ToString(), x.TrangThai.ToString());
}
