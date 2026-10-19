using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Academic.Services;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Grades.Services;

public class CanhBaoHocVuService(AppDbContext db, IBangDiemService bangDiem) : ICanhBaoHocVuService
{
    // Ngưỡng mặc định khi chưa cấu hình quy chế (MaQuyChe.CanhBao*): GPA học kỳ 1 của năm học đầu tiên
    // phải từ 0.80, các học kỳ chính khác từ 1.00; mỗi học kỳ chính phải đăng ký tối thiểu 12 tín chỉ.
    private const decimal NguongGpaHocKyDauTienMacDinh = 0.80m;
    private const decimal NguongGpaMacDinh = 1.00m;
    private const int SoTinChiToiThieuMacDinh = 12;
    private const string GhiChu =
        "Đánh giá tự động; điều kiện tín chỉ cần Phòng đào tạo xác nhận các trường hợp được công nhận tín chỉ.";

    private readonly AppDbContext _db = db;
    private readonly IBangDiemService _bangDiem = bangDiem;

    public async Task<CanhBaoHocVuDto> GetCanhBaoAsync(int sinhVienId, int hocKyId)
    {
        var hocKy = await GetHocKyAsync(hocKyId);
        var sinhVien = await ProjectSinhVien(_db.Set<SinhVien>().AsNoTracking().Where(x => x.Id == sinhVienId))
            .FirstOrDefaultAsync()
            ?? throw AppException.NotFound("sinh viên");
        var bangDiem = await _bangDiem.GetBangDiemAsync(sinhVienId);
        return DanhGia(sinhVien, hocKy, bangDiem, await GetNguongAsync());
    }

    public async Task<PagedResult<CanhBaoHocVuDto>> GetDanhSachCanhBaoAsync(int hocKyId, PagedQuery query)
    {
        var hocKy = await GetHocKyAsync(hocKyId);

        // Chỉ xét sinh viên có lượt đăng ký còn hiệu lực ở học kỳ này.
        var sinhVienIds = _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.TrangThai == TrangThaiDangKy.DaDangKy)
            .Join(_db.Set<LopHocPhan>().AsNoTracking().Where(x => x.HocKyId == hocKyId),
                dk => dk.LopHocPhanId, lop => lop.Id, (dk, lop) => dk.SinhVienId)
            .Distinct();
        var sinhVienQuery = _db.Set<SinhVien>().AsNoTracking().Where(x => sinhVienIds.Contains(x.Id));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            sinhVienQuery = sinhVienQuery.Where(x => x.MSSV.Contains(search) || x.HoTen.Contains(search));
        }
        var sinhVien = await ProjectSinhVien(sinhVienQuery.OrderBy(x => x.MSSV).ThenBy(x => x.Id)).ToListAsync();

        // Phải tính xong bảng điểm mới biết ai bị cảnh báo, nên lọc và phân trang trong bộ nhớ.
        var bangDiemTheoSinhVien = await _bangDiem.GetBangDiemNhieuSinhVienAsync(sinhVien.Select(x => x.Id).ToArray());
        var nguong = await GetNguongAsync();
        var canhBao = sinhVien
            .Select(sv => DanhGia(sv, hocKy, bangDiemTheoSinhVien[sv.Id], nguong))
            .Where(x => x.TrangThaiCanhBao == TrangThaiCanhBao.CanhBao)
            .ToList();
        return canhBao.ToPagedResult(query);
    }

    private sealed record SinhVienCanhBao(int Id, string MSSV, string HoTen, int? KhoaTuyen);

    /// <summary>NamBatDau: năm bắt đầu của năm học, dùng để so với khóa tuyển của sinh viên.</summary>
    private sealed record HocKyCanhBao(int Id, string Ten, int SoThuTu, DateOnly NgayKetThuc,
        string TenNamHoc, int NamBatDau);

    private async Task<HocKyCanhBao> GetHocKyAsync(int hocKyId) =>
        await (from hk in _db.Set<HocKy>().AsNoTracking()
               where hk.Id == hocKyId
               join nh in _db.Set<NamHoc>().AsNoTracking() on hk.NamHocId equals nh.Id
               select new HocKyCanhBao(hk.Id, hk.Ten, hk.SoThuTu, hk.NgayKetThuc, nh.Ten, nh.NgayBatDau.Year))
            .FirstOrDefaultAsync()
        ?? throw AppException.NotFound("học kỳ");

    // Khóa tuyển của sinh viên lấy từ lớp hành chính (sinh viên chưa có lớp thì KhoaTuyen = null).
    private IQueryable<SinhVienCanhBao> ProjectSinhVien(IQueryable<SinhVien> sinhVien) =>
        from sv in sinhVien
        join lop in _db.Set<LopHanhChinh>().AsNoTracking() on sv.LopHanhChinhId equals (int?)lop.Id into nhomLop
        from lop in nhomLop.DefaultIfEmpty()
        select new SinhVienCanhBao(sv.Id, sv.MSSV, sv.HoTen, lop == null ? null : lop.KhoaTuyen);

    private sealed record NguongCanhBao(decimal GpaHocKyDauTien, decimal Gpa, int SoTinChiToiThieu);

    private async Task<NguongCanhBao> GetNguongAsync() => new(
        await _db.GetThamSoAsync(MaQuyChe.CanhBaoGpaHocKyDauTien, NguongGpaHocKyDauTienMacDinh),
        await _db.GetThamSoAsync(MaQuyChe.CanhBaoGpaToiThieu, NguongGpaMacDinh),
        await _db.GetThamSoNguyenAsync(MaQuyChe.CanhBaoTinChiToiThieu, SoTinChiToiThieuMacDinh));

    private static CanhBaoHocVuDto DanhGia(
        SinhVienCanhBao sv, HocKyCanhBao hocKy, BangDiemSinhVienDto bangDiem, NguongCanhBao nguong)
    {
        var diemHocKy = bangDiem.DanhSachHocKy.FirstOrDefault(x => x.HocKyId == hocKy.Id);
        var monHoc = diemHocKy?.DanhSachMonHoc ?? [];
        var tongTinChiDangKy = monHoc.Sum(x => x.SoTinChi);
        // DiemHe4 chỉ có khi điểm đã khóa; môn có điểm tổng kết dưới mức đạt là môn không đạt.
        var monKhongDat = monHoc
            .Where(x => x.DiemHe4 is not null && !GradeCalculator.LaDiemDat(x.DiemTongKet!.Value))
            .ToList();

        CanhBaoHocVuDto KetQua(TrangThaiCanhBao trangThai, List<string> lyDo) =>
            new(sv.Id, sv.MSSV, sv.HoTen, hocKy.Id, hocKy.Ten, hocKy.TenNamHoc,
                diemHocKy?.Gpa, diemHocKy?.Cpa, tongTinChiDangKy,
                monKhongDat.Count, monKhongDat.Sum(x => x.SoTinChi), trangThai, lyDo, GhiChu);

        var lyDoChuaDuDuLieu = TimLyDoChuaDuDuLieu(sv, hocKy, diemHocKy);
        if (lyDoChuaDuDuLieu.Count > 0)
            return KetQua(TrangThaiCanhBao.ChuaDuDuLieu, lyDoChuaDuDuLieu);

        // Học kỳ 1 của năm học đầu tiên (năm bắt đầu = khóa tuyển) dùng ngưỡng thấp hơn.
        var nguongGpa = hocKy.SoThuTu == 1 && hocKy.NamBatDau == sv.KhoaTuyen ? nguong.GpaHocKyDauTien : nguong.Gpa;
        var lyDoCanhBao = new List<string>();
        if (diemHocKy!.Gpa < nguongGpa)
            lyDoCanhBao.Add($"GPA học kỳ {diemHocKy.Gpa:0.00} thấp hơn mức {nguongGpa:0.00}.");
        if (tongTinChiDangKy < nguong.SoTinChiToiThieu)
            lyDoCanhBao.Add($"Sinh viên chỉ đăng ký {tongTinChiDangKy} tín chỉ, thấp hơn mức tối thiểu {nguong.SoTinChiToiThieu} tín chỉ.");

        return KetQua(lyDoCanhBao.Count > 0 ? TrangThaiCanhBao.CanhBao : TrangThaiCanhBao.KhongCanhBao, lyDoCanhBao);
    }

    /// <summary>Liệt kê lý do chưa thể kết luận cảnh báo; danh sách rỗng nghĩa là đã đủ dữ liệu.</summary>
    private static List<string> TimLyDoChuaDuDuLieu(SinhVienCanhBao sv, HocKyCanhBao hocKy, BangDiemHocKyDto? diemHocKy)
    {
        var lyDo = new List<string>();
        // Xét thứ tự 1/2 như hai kỳ chính; các thứ tự khác chưa đủ căn cứ để xét riêng.
        if (hocKy.SoThuTu is not (1 or 2))
            lyDo.Add("Học kỳ này chưa được xác định là học kỳ chính để xét cảnh báo.");
        if (hocKy.NgayKetThuc >= HomNayTheoGioVietNam())
            lyDo.Add("Học kỳ chưa kết thúc; chưa thể kết luận cảnh báo chính thức.");
        if (diemHocKy is null || diemHocKy.DanhSachMonHoc.Count == 0)
            lyDo.Add("Sinh viên chưa có đăng ký còn hiệu lực trong học kỳ.");
        if (diemHocKy?.Gpa is null)
            lyDo.Add("Chưa có GPA học kỳ từ kết quả đã khóa.");
        if (diemHocKy?.GpaTamTinh == true)
            lyDo.Add("Học kỳ còn học phần chưa có điểm được khóa.");
        if (sv.KhoaTuyen is null or <= 0 || hocKy.NamBatDau < sv.KhoaTuyen)
            lyDo.Add("Chưa xác định được khóa tuyển để áp dụng ngưỡng GPA.");
        return lyDo;
    }

    // Việt Nam dùng UTC+7 quanh năm (không đổi giờ theo mùa), nên cộng thẳng 7 giờ là đủ.
    private static DateOnly HomNayTheoGioVietNam() => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
}
