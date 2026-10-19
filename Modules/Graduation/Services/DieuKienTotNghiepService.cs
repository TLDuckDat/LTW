using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
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
        var items = new List<DieuKienTotNghiepDto>();
        foreach (var id in ids)
            items.Add(await GetBySinhVienIdAsync(id));

        return new PagedResult<DieuKienTotNghiepDto>
            { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<DieuKienTotNghiepDto> GetBySinhVienIdAsync(int sinhVienId)
    {
        var sv = await _db.Set<SinhVien>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sinhVienId)
            ?? throw AppException.NotFound("sinh viên");

        var (chuongTrinh, lyDoChuongTrinh) = await TimChuongTrinhAsync(sv);

        BangDiemSinhVienDto? bangDiem = null;
        string? loiBangDiem = null;
        try { bangDiem = await _bangDiem.GetBangDiemAsync(sinhVienId); }
        catch (AppException ex) { loiBangDiem = $"Không thể tính bảng điểm: {ex.Message}"; }

        var thucTap = await _db.Set<ThucTap>().AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId && x.TrangThai != TrangThaiThucTap.DaHuy)
            .OrderBy(x => x.Id).ToListAsync();
        var deTai = await _db.Set<DeTaiKhoaLuan>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.SinhVienId == sinhVienId);

        int? tinChiYeuCau = chuongTrinh?.TongSoTinChi > 0 ? chuongTrinh.TongSoTinChi : null;
        var tinChiTichLuy = bangDiem?.TongTinChiTichLuyDat;
        int? tinChiConThieu = tinChiYeuCau is int can && tinChiTichLuy is int dat
            ? Math.Max(0, can - dat) : null;

        var dieuKien = new List<DieuKienDto>
        {
            XetTrangThai(sv.TrangThai),
            XetChuongTrinh(chuongTrinh, lyDoChuongTrinh),
            XetTinChi(tinChiYeuCau, tinChiTichLuy, loiBangDiem),
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
            new ThucTapTotNghiepDto(chuongTrinh?.YeuCauThucTap, thucTap.Count,
                thucTapDuyNhat?.Diem, chuongTrinh?.DiemThucTapToiThieu,
                thucTapDuyNhat?.TrangThai == TrangThaiThucTap.HoanThanh && thucTapDuyNhat.Diem is not null),
            new KhoaLuanTotNghiepDto(chuongTrinh?.YeuCauKhoaLuan, deTai?.Id,
                deTai?.HoiDongBaoVeId, deTai?.Diem, chuongTrinh?.DiemBaoVeToiThieu,
                deTai?.Diem is not null),
            dieuKien, KetLuan(dieuKien));
    }

    /// <summary>Tìm chương trình đào tạo theo ngành và khóa tuyển (lấy từ lớp hành chính) của sinh viên.
    /// Luôn trả kèm lý do để giải thích khi chưa xác định được chương trình.</summary>
    private async Task<(ChuongTrinhDaoTao? ChuongTrinh, string LyDo)> TimChuongTrinhAsync(SinhVien sv)
    {
        var lop = sv.LopHanhChinhId is int lopId
            ? await _db.Set<LopHanhChinh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lopId)
            : null;
        if (lop is null)
            return (null, "Sinh viên chưa có lớp hành chính để xác định khóa tuyển.");
        if (lop.NganhId != sv.NganhId || lop.KhoaTuyen <= 0)
            return (null, "Ngành hoặc khóa tuyển của lớp hành chính không hợp lệ với sinh viên.");

        var matches = await _db.Set<ChuongTrinhDaoTao>().AsNoTracking()
            .Where(x => x.NganhId == sv.NganhId && x.KhoaTuyen == lop.KhoaTuyen)
            .ToListAsync();
        return matches.Count switch
        {
            0 => (null, "Chưa có chương trình đào tạo cho ngành và khóa tuyển của sinh viên."),
            1 => (matches[0], "Đã xác định chương trình đào tạo theo ngành và khóa tuyển."),
            _ => (null, "Có nhiều chương trình đào tạo cùng ngành và khóa tuyển; chưa biết áp dụng chương trình nào.")
        };
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
