using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;
using static StudentManagement.Modules.Graduation.Dtos.TrangThaiDieuKien;

namespace StudentManagement.Modules.Graduation.Services;

public class DieuKienTotNghiepService(AppDbContext db, IBangDiemService bangDiem)
    : IDieuKienTotNghiepService
{
    private readonly AppDbContext _db = db;
    private readonly IBangDiemService _bangDiem = bangDiem;

    public async Task<PagedResult<DieuKienTotNghiepDto>> GetPagedAsync(PagedQuery query)
    {
        var students = _db.Set<SinhVien>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            students = students.Where(x => x.MSSV.Contains(search) || x.HoTen.Contains(search));
        }

        var total = await students.CountAsync();
        var ids = await students.OrderBy(x => x.MSSV).ThenBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => x.Id).ToListAsync();
        var ketQua = await XetNhieuSinhVienAsync(ids);

        return new PagedResult<DieuKienTotNghiepDto>
            { Items = ids.Select(id => ketQua[id]).ToList(), Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<DieuKienTotNghiepDto> GetBySinhVienIdAsync(int sinhVienId) =>
        (await XetNhieuSinhVienAsync([sinhVienId])).GetValueOrDefault(sinhVienId)
            ?? throw AppException.NotFound("sinh viên");

    private sealed record MonBatBuoc(int ChuongTrinhDaoTaoId, int MonHocId, string Ma, string Ten);

    /// <summary>
    /// Xét điều kiện tốt nghiệp cho nhiều sinh viên với số truy vấn cố định (không phụ thuộc số sinh viên).
    /// Sinh viên không tồn tại sẽ không có trong kết quả.
    /// </summary>
    private async Task<Dictionary<int, DieuKienTotNghiepDto>> XetNhieuSinhVienAsync(IReadOnlyList<int> sinhVienIds)
    {
        var dsSinhVien = await _db.Set<SinhVien>().AsNoTracking()
            .Where(x => sinhVienIds.Contains(x.Id)).ToListAsync();
        if (dsSinhVien.Count == 0) return [];
        var ids = dsSinhVien.Select(x => x.Id).ToArray();

        var lopIds = dsSinhVien.Where(x => x.LopHanhChinhId != null).Select(x => x.LopHanhChinhId!.Value).Distinct().ToList();
        var lop = await _db.Set<LopHanhChinh>().AsNoTracking()
            .Where(x => lopIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        var nganhIds = dsSinhVien.Select(x => x.NganhId).Distinct().ToList();
        var chuongTrinh = (await _db.Set<ChuongTrinhDaoTao>().AsNoTracking()
                .Where(x => nganhIds.Contains(x.NganhId)).ToListAsync())
            .ToLookup(x => (x.NganhId, x.KhoaTuyen));
        var ctdtIds = chuongTrinh.SelectMany(g => g).Select(x => x.Id).ToList();
        var monBatBuoc = (await (
                from ctm in _db.Set<ChuongTrinhDaoTaoMonHoc>().AsNoTracking()
                join mh in _db.Set<MonHoc>().AsNoTracking() on ctm.MonHocId equals mh.Id
                where ctm.BatBuoc && ctdtIds.Contains(ctm.ChuongTrinhDaoTaoId)
                orderby mh.Ma
                select new MonBatBuoc(ctm.ChuongTrinhDaoTaoId, mh.Id, mh.Ma, mh.Ten))
                .ToListAsync())
            .ToLookup(x => x.ChuongTrinhDaoTaoId);

        var (bangDiem, loiBangDiem) = await TinhBangDiemAsync(ids);

        var thucTap = (await _db.Set<ThucTap>().AsNoTracking()
                .Where(x => ids.Contains(x.SinhVienId) && x.TrangThai != TrangThaiThucTap.DaHuy)
                .OrderBy(x => x.Id).ToListAsync())
            .ToLookup(x => x.SinhVienId);
        var deTai = await _db.Set<DeTaiKhoaLuan>().AsNoTracking()
            .Where(x => x.SinhVienId != null && ids.Contains(x.SinhVienId.Value))
            .ToDictionaryAsync(x => x.SinhVienId!.Value);

        return dsSinhVien.ToDictionary(sv => sv.Id, sv =>
        {
            var lopCuaSinhVien = sv.LopHanhChinhId is int lopId ? lop.GetValueOrDefault(lopId) : null;
            var (ct, lyDoChuongTrinh) = TimChuongTrinh(sv, lopCuaSinhVien, chuongTrinh);
            return Xet(sv, ct, lyDoChuongTrinh,
                ct is null ? [] : monBatBuoc[ct.Id].ToList(),
                bangDiem.GetValueOrDefault(sv.Id), loiBangDiem.GetValueOrDefault(sv.Id),
                thucTap[sv.Id].ToList(), deTai.GetValueOrDefault(sv.Id));
        });
    }

    /// <summary>Tính bảng điểm theo lô; nếu dữ liệu của ai đó lỗi thì tính lại từng người để chỉ người đó bị ảnh hưởng.</summary>
    private async Task<(Dictionary<int, BangDiemSinhVienDto> BangDiem, Dictionary<int, string> Loi)> TinhBangDiemAsync(int[] ids)
    {
        try
        {
            return (await _bangDiem.GetBangDiemNhieuSinhVienAsync(ids), []);
        }
        catch (AppException)
        {
            var bangDiem = new Dictionary<int, BangDiemSinhVienDto>();
            var loi = new Dictionary<int, string>();
            foreach (var id in ids)
            {
                try { bangDiem[id] = await _bangDiem.GetBangDiemAsync(id); }
                catch (AppException ex) { loi[id] = $"Không thể tính bảng điểm: {ex.Message}"; }
            }
            return (bangDiem, loi);
        }
    }

    private static DieuKienTotNghiepDto Xet(
        SinhVien sv, ChuongTrinhDaoTao? chuongTrinh, string lyDoChuongTrinh, IReadOnlyList<MonBatBuoc> monBatBuoc,
        BangDiemSinhVienDto? bangDiem, string? loiBangDiem, IReadOnlyList<ThucTap> thucTap, DeTaiKhoaLuan? deTai)
    {
        int? tinChiYeuCau = chuongTrinh?.TongSoTinChi > 0 ? chuongTrinh.TongSoTinChi : null;
        var tinChiTichLuy = bangDiem?.TongTinChiTichLuyDat;
        int? tinChiConThieu = tinChiYeuCau is int can && tinChiTichLuy is int dat
            ? Math.Max(0, can - dat) : null;

        // Môn đã đạt: có lượt học đã khóa điểm với điểm tổng kết từ mức đạt
        var monDaDat = bangDiem?.DanhSachHocKy
            .SelectMany(x => x.DanhSachMonHoc)
            .Where(x => x.DaKhoa && x.DiemTongKet is decimal diem && GradeCalculator.LaDiemDat(diem))
            .Select(x => x.MonHocId)
            .ToHashSet() ?? [];
        var monConThieu = monBatBuoc.Where(x => !monDaDat.Contains(x.MonHocId)).Select(x => $"{x.Ma} - {x.Ten}").ToList();

        var dieuKien = new List<DieuKienDto>
        {
            XetTrangThai(sv.TrangThai),
            XetChuongTrinh(chuongTrinh, lyDoChuongTrinh),
            XetTinChi(tinChiYeuCau, tinChiTichLuy, loiBangDiem),
            XetMonBatBuoc(chuongTrinh, monBatBuoc.Count, monConThieu, loiBangDiem),
            XetCpa(chuongTrinh?.CpaToiThieu, bangDiem?.Cpa, loiBangDiem),
            XetThucTap(chuongTrinh, thucTap),
            XetKhoaLuan(chuongTrinh, deTai)
        };

        var thucTapDuyNhat = thucTap.Count == 1 ? thucTap[0] : null;
        return new DieuKienTotNghiepDto(
            sv.Id, sv.MSSV, sv.HoTen, sv.TrangThai.ToString(),
            chuongTrinh?.Id, chuongTrinh?.Ten,
            tinChiYeuCau, tinChiTichLuy, tinChiConThieu,
            bangDiem?.Cpa, chuongTrinh?.CpaToiThieu,
            bangDiem?.Cpa is decimal cpa ? GradeCalculator.XepLoai(cpa).Ten : null,
            new ThucTapTotNghiepDto(chuongTrinh?.YeuCauThucTap, thucTap.Count,
                thucTapDuyNhat?.Diem, chuongTrinh?.DiemThucTapToiThieu,
                thucTapDuyNhat?.TrangThai == TrangThaiThucTap.HoanThanh && thucTapDuyNhat.Diem is not null),
            new KhoaLuanTotNghiepDto(chuongTrinh?.YeuCauKhoaLuan, deTai?.Id,
                deTai?.HoiDongBaoVeId, deTai?.Diem, chuongTrinh?.DiemBaoVeToiThieu,
                deTai?.Diem is not null),
            new MonBatBuocTotNghiepDto(monBatBuoc.Count, monBatBuoc.Count - monConThieu.Count, monConThieu),
            dieuKien, KetLuan(dieuKien));
    }

    /// <summary>Tìm chương trình đào tạo theo ngành và khóa tuyển (lấy từ lớp hành chính) của sinh viên.
    /// Luôn trả kèm lý do để giải thích khi chưa xác định được chương trình.</summary>
    private static (ChuongTrinhDaoTao? ChuongTrinh, string LyDo) TimChuongTrinh(
        SinhVien sv, LopHanhChinh? lop, ILookup<(int NganhId, int KhoaTuyen), ChuongTrinhDaoTao> chuongTrinh)
    {
        if (lop is null)
            return (null, "Sinh viên chưa có lớp hành chính để xác định khóa tuyển.");
        if (lop.NganhId != sv.NganhId || lop.KhoaTuyen <= 0)
            return (null, "Ngành hoặc khóa tuyển của lớp hành chính không hợp lệ với sinh viên.");

        var matches = chuongTrinh[(sv.NganhId, lop.KhoaTuyen)].ToList();
        return matches.Count switch
        {
            0 => (null, "Chưa có chương trình đào tạo cho ngành và khóa tuyển của sinh viên."),
            1 => (matches[0], "Đã xác định chương trình đào tạo theo ngành và khóa tuyển."),
            _ => (null, "Có nhiều chương trình đào tạo cùng ngành và khóa tuyển; chưa biết áp dụng chương trình nào.")
        };
    }

    private static DieuKienDto XetMonBatBuoc(
        ChuongTrinhDaoTao? chuongTrinh, int soMonBatBuoc, IReadOnlyList<string> monConThieu, string? loiBangDiem)
    {
        static DieuKienDto KetQua(TrangThaiDieuKien trangThai, string giaiThich) =>
            new("MonBatBuoc", "Môn học bắt buộc", trangThai, giaiThich);

        if (chuongTrinh is null)
            return KetQua(ChuaXacDinh, "Chưa xác định được chương trình đào tạo.");
        if (soMonBatBuoc == 0)
            return KetQua(ChuaXacDinh, "Chương trình đào tạo chưa khai báo môn học bắt buộc.");
        if (loiBangDiem is not null)
            return KetQua(ChuaXacDinh, loiBangDiem);
        if (monConThieu.Count == 0)
            return KetQua(Dat, $"Đã đạt đủ {soMonBatBuoc} môn bắt buộc.");

        const int SoMonHienThi = 5;
        var danhSach = string.Join(", ", monConThieu.Take(SoMonHienThi));
        var conLai = monConThieu.Count > SoMonHienThi ? $" và {monConThieu.Count - SoMonHienThi} môn khác" : "";
        return KetQua(ChuaDat, $"Còn {monConThieu.Count}/{soMonBatBuoc} môn bắt buộc chưa đạt: {danhSach}{conLai}.");
    }

    private static KetLuanTotNghiep KetLuan(IReadOnlyList<DieuKienDto> dieuKien)
    {
        if (dieuKien.Any(x => x.TrangThai == ChuaDat)) return KetLuanTotNghiep.KhongDuDieuKien;
        if (dieuKien.All(x => x.TrangThai == Dat)) return KetLuanTotNghiep.DuDieuKien;
        return KetLuanTotNghiep.ChuaTheKetLuan;
    }

    private static DieuKienDto XetTrangThai(TrangThaiSinhVien trangThai)
    {
        static DieuKienDto KetQua(TrangThaiDieuKien trangThai, string giaiThich) =>
            new("TrangThaiHocTap", "Trạng thái học tập", trangThai, giaiThich);

        return trangThai switch
        {
            TrangThaiSinhVien.DangHoc => KetQua(Dat, "Sinh viên đang học."),
            TrangThaiSinhVien.BaoLuu or TrangThaiSinhVien.ThoiHoc =>
                KetQua(ChuaDat, $"Sinh viên đang ở trạng thái {trangThai}."),
            _ => KetQua(ChuaXacDinh,
                "Sinh viên đã tốt nghiệp hoặc có trạng thái chưa được quy định để kiểm tra lại.")
        };
    }

    private static DieuKienDto XetChuongTrinh(ChuongTrinhDaoTao? chuongTrinh, string lyDo) =>
        new("ChuongTrinhDaoTao", "Chương trình đào tạo",
            chuongTrinh is null ? ChuaXacDinh : Dat, lyDo);

    private static DieuKienDto XetTinChi(int? yeuCau, int? tichLuy, string? loiBangDiem)
    {
        static DieuKienDto KetQua(TrangThaiDieuKien trangThai, string giaiThich) =>
            new("TinChi", "Tín chỉ tích lũy", trangThai, giaiThich);

        if (yeuCau is null)
            return KetQua(ChuaXacDinh, "Chưa có tổng tín chỉ yêu cầu hợp lệ trong chương trình đào tạo.");
        if (tichLuy is null)
            return KetQua(ChuaXacDinh, loiBangDiem ?? "Chưa có dữ liệu tín chỉ tích lũy.");
        return tichLuy >= yeuCau
            ? KetQua(Dat, $"Đã tích lũy {tichLuy}/{yeuCau} tín chỉ.")
            : KetQua(ChuaDat, $"Đã tích lũy {tichLuy}/{yeuCau} tín chỉ; còn thiếu {yeuCau - tichLuy}.");
    }

    private static DieuKienDto XetCpa(decimal? yeuCau, decimal? cpa, string? loiBangDiem)
    {
        static DieuKienDto KetQua(TrangThaiDieuKien trangThai, string giaiThich) =>
            new("Cpa", "Điểm trung bình tích lũy", trangThai, giaiThich);

        if (yeuCau is null)
            return KetQua(ChuaXacDinh, "Chương trình đào tạo chưa quy định CPA tối thiểu.");
        if (cpa is null)
            return KetQua(ChuaXacDinh, loiBangDiem ?? "Chưa có CPA từ điểm học phần đã khóa.");
        return cpa >= yeuCau
            ? KetQua(Dat, $"CPA {cpa:0.00} đạt mức {yeuCau:0.00}.")
            : KetQua(ChuaDat, $"CPA {cpa:0.00} dưới mức {yeuCau:0.00}.");
    }

    private static DieuKienDto XetThucTap(ChuongTrinhDaoTao? ct, IReadOnlyList<ThucTap> records)
    {
        static DieuKienDto KetQua(TrangThaiDieuKien trangThai, string giaiThich) =>
            new("ThucTap", "Thực tập", trangThai, giaiThich);

        if (ct?.YeuCauThucTap is null)
            return KetQua(ChuaXacDinh, "Chương trình đào tạo chưa quy định có bắt buộc thực tập hay không.");
        if (ct.YeuCauThucTap == false)
            return KetQua(Dat, "Chương trình đào tạo không bắt buộc thực tập.");
        if (records.Count == 0)
            return KetQua(ChuaDat, "Chưa có đăng ký thực tập còn hiệu lực.");
        if (records.Count > 1)
            return KetQua(ChuaXacDinh, "Có nhiều kỳ thực tập; chưa có quy định chọn kết quả để xét tốt nghiệp.");
        if (records[0].TrangThai != TrangThaiThucTap.HoanThanh || records[0].Diem is null)
            return KetQua(ChuaDat, "Thực tập chưa có kết quả hoàn thành.");
        if (ct.DiemThucTapToiThieu is null)
            return KetQua(ChuaXacDinh, "Chương trình đào tạo chưa quy định điểm thực tập tối thiểu.");
        return records[0].Diem >= ct.DiemThucTapToiThieu
            ? KetQua(Dat, "Kết quả thực tập đạt điểm yêu cầu.")
            : KetQua(ChuaDat, "Điểm thực tập dưới mức yêu cầu.");
    }

    private static DieuKienDto XetKhoaLuan(ChuongTrinhDaoTao? ct, DeTaiKhoaLuan? deTai)
    {
        static DieuKienDto KetQua(TrangThaiDieuKien trangThai, string giaiThich) =>
            new("KhoaLuan", "Khóa luận/đồ án", trangThai, giaiThich);

        if (ct?.YeuCauKhoaLuan is null)
            return KetQua(ChuaXacDinh, "Chương trình đào tạo chưa quy định có bắt buộc khóa luận/đồ án hay không.");
        if (ct.YeuCauKhoaLuan == false)
            return KetQua(Dat, "Chương trình đào tạo không bắt buộc khóa luận/đồ án.");
        if (deTai is null)
            return KetQua(ChuaDat, "Sinh viên chưa có đề tài.");
        if (deTai.HoiDongBaoVeId is null || deTai.Diem is null)
            return KetQua(ChuaDat, "Đề tài chưa có hội đồng hoặc kết quả bảo vệ.");
        if (ct.DiemBaoVeToiThieu is null)
            return KetQua(ChuaXacDinh, "Chương trình đào tạo chưa quy định điểm bảo vệ tối thiểu.");
        return deTai.Diem >= ct.DiemBaoVeToiThieu
            ? KetQua(Dat, "Kết quả bảo vệ đạt điểm yêu cầu.")
            : KetQua(ChuaDat, "Điểm bảo vệ dưới mức yêu cầu.");
    }
}
