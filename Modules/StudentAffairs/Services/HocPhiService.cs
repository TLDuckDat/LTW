using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Academic.Services;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Services;

public class HocPhiService(AppDbContext db, NhatKyContext nhatKy) : IHocPhiService
{
    /// <summary>Đơn giá mặc định khi chưa cấu hình quy chế DON_GIA_TIN_CHI.</summary>
    private const decimal DonGiaTinChiMacDinh = 600_000m;

    private readonly AppDbContext _db = db;
    private readonly NhatKyContext _nhatKy = nhatKy;

    public Task<PagedResult<HocPhiDto>> GetPagedAsync(HocPhiQuery query)
    {
        var data = _db.Set<HocPhi>().AsNoTracking();
        if (query.SinhVienId is int sinhVienId) data = data.Where(x => x.SinhVienId == sinhVienId);
        if (query.HocKyId is int hocKyId) data = data.Where(x => x.HocKyId == hocKyId);
        if (query.ConNo is bool conNo) data = data.Where(x => (x.SoTien > x.SoTienDaNop) == conNo);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var sinhVien = _db.SinhVienPhuHop(query.Search);
            data = data.Where(x => sinhVien.Contains(x.SinhVienId));
        }

        return data.OrderByDescending(x => x.HocKyId).ThenBy(x => x.SinhVienId).ToPagedResultAsync(query, Project);
    }

    public async Task<HocPhiDto> GetByIdAsync(int id, int? chiSinhVienId = null) =>
        await Project(_db.Set<HocPhi>().AsNoTracking()
                .Where(x => x.Id == id && (chiSinhVienId == null || x.SinhVienId == chiSinhVienId)))
            .FirstOrDefaultAsync()
        ?? throw AppException.NotFound("học phí");

    // SA-02 Tính (hoặc tính lại) học phí cho mọi sinh viên có đăng ký trong học kỳ.
    // Tính lại chỉ đổi số phải nộp, không động tới số đã nộp; nộp thừa thể hiện ở SoTienThua.
    public async Task<TinhHocPhiResultDto> TinhHocPhiAsync(int hocKyId)
    {
        var hocKy = await _db.GetHocKyAsync(hocKyId);
        var donGia = await _db.GetThamSoAsync(MaQuyChe.DonGiaTinChi, DonGiaTinChiMacDinh);
        if (donGia < 0)
            throw AppException.Conflict("Đơn giá tín chỉ trong quy chế không được âm.");

        var tinChi = await (
                from dk in _db.Set<DangKyHocPhan>().AsNoTracking()
                join lop in _db.Set<LopHocPhan>().AsNoTracking() on dk.LopHocPhanId equals lop.Id
                join mh in _db.Set<MonHoc>().AsNoTracking() on lop.MonHocId equals mh.Id
                where lop.HocKyId == hocKyId && lop.TrangThai != TrangThaiLopHocPhan.DaHuy
                      && dk.TrangThai == TrangThaiDangKy.DaDangKy
                group mh by dk.SinhVienId into g
                select new { SinhVienId = g.Key, TongTinChi = g.Sum(x => x.SoTinChi) })
            .ToDictionaryAsync(x => x.SinhVienId, x => x.TongTinChi);
        var tyLeMienGiam = await _db.Set<MienGiam>().AsNoTracking()
            .Where(x => x.HocKyId == hocKyId)
            .ToDictionaryAsync(x => x.SinhVienId, x => x.TyLe);
        var hienCo = await _db.Set<HocPhi>()
            .Where(x => x.HocKyId == hocKyId)
            .ToDictionaryAsync(x => x.SinhVienId);

        int soTaoMoi = 0, soCapNhat = 0, soKhongThayDoi = 0;
        // Sinh viên đã hủy hết học phần vẫn giữ bản ghi (số phải nộp về 0) để không mất số đã nộp
        foreach (var sinhVienId in tinChi.Keys.Union(hienCo.Keys))
        {
            var soTinChi = tinChi.GetValueOrDefault(sinhVienId);
            var tyLe = tyLeMienGiam.GetValueOrDefault(sinhVienId);
            var soTien = decimal.Round(soTinChi * donGia * (1m - tyLe / 100m), 2, MidpointRounding.AwayFromZero);

            if (!hienCo.TryGetValue(sinhVienId, out var hocPhi))
            {
                hocPhi = new HocPhi { SinhVienId = sinhVienId, HocKyId = hocKyId };
                _db.Add(hocPhi);
                hienCo[sinhVienId] = hocPhi;
                soTaoMoi++;
            }
            else if (hocPhi.SoTien == soTien)
            {
                soKhongThayDoi++;
            }
            else
            {
                soCapNhat++;
            }

            hocPhi.SoTien = soTien;
            hocPhi.DaThanhToan = hocPhi.SoTienDaNop >= hocPhi.SoTien;
        }

        await _db.SaveChangesAsync();
        _nhatKy.Ghi($"{hocKy.Ten}: đơn giá {donGia:0}, tạo {soTaoMoi}, cập nhật {soCapNhat} khoản học phí");
        return new TinhHocPhiResultDto(
            hocKy.Id, hocKy.Ten, donGia, hienCo.Count, soTaoMoi, soCapNhat, soKhongThayDoi,
            hienCo.Values.Count(x => x.SoTienDaNop > x.SoTien), hienCo.Values.Sum(x => x.SoTien));
    }

    // SA-03 Ghi nhận một lần nộp; không cho nộp vượt số còn nợ
    public async Task<ThanhToanResultDto> ThanhToanAsync(int id, ThanhToanRequest? request, int nguoiThuId)
    {
        var hocPhi = await _db.Set<HocPhi>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("học phí");
        var conNo = hocPhi.SoTien - hocPhi.SoTienDaNop;
        if (conNo <= 0)
            throw AppException.Conflict("Khoản học phí này đã được nộp đủ.");

        var soTien = request?.SoTien ?? conNo;
        if (decimal.Round(soTien, 2) != soTien)
            throw AppException.BadRequest("Số tiền nộp chỉ được có tối đa 2 chữ số thập phân.");
        if (soTien > conNo)
            throw AppException.BadRequest($"Số tiền nộp ({soTien:0.##}) vượt số còn nợ ({conNo:0.##}).");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        // Cập nhật có điều kiện: hai lần nộp cùng lúc không thể cùng vượt số còn nợ
        var updated = await _db.Set<HocPhi>()
            .Where(x => x.Id == id && x.SoTien - x.SoTienDaNop >= soTien)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.SoTienDaNop, x => x.SoTienDaNop + soTien)
                .SetProperty(x => x.DaThanhToan, x => x.SoTienDaNop + soTien >= x.SoTien)
                .SetProperty(x => x.UpdatedAt, now));
        if (updated == 0)
            throw AppException.Conflict("Học phí vừa được cập nhật bởi thao tác khác, hãy tải lại rồi thử lại.");

        var thanhToan = new ThanhToanHocPhi
        {
            HocPhiId = id,
            SoTien = soTien,
            NgayThanhToan = now,
            HinhThuc = request?.HinhThuc?.Trim(),
            GhiChu = request?.GhiChu?.Trim(),
            NguoiThuId = nguoiThuId
        };
        _db.Add(thanhToan);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        _nhatKy.Ghi($"nộp {soTien:0.##} cho sinh viên {hocPhi.SinhVienId}, còn nợ {conNo - soTien:0.##}" + (string.IsNullOrWhiteSpace(thanhToan.HinhThuc) ? "" : $", hình thức {thanhToan.HinhThuc}"));

        var lichSu = await ProjectThanhToan(_db.Set<ThanhToanHocPhi>().AsNoTracking().Where(x => x.Id == thanhToan.Id))
            .FirstAsync();
        return new ThanhToanResultDto(await GetByIdAsync(id), lichSu);
    }

    public async Task<IReadOnlyList<ThanhToanHocPhiDto>> GetLichSuThanhToanAsync(int id, int? chiSinhVienId = null)
    {
        await GetByIdAsync(id, chiSinhVienId);
        // Mỗi khoản chỉ có vài lần nộp nên sắp xếp trong bộ nhớ
        var lichSu = await ProjectThanhToan(_db.Set<ThanhToanHocPhi>().AsNoTracking().Where(x => x.HocPhiId == id))
            .ToListAsync();
        return lichSu.OrderBy(x => x.NgayThanhToan).ThenBy(x => x.Id).ToList();
    }

    private IQueryable<HocPhiDto> Project(IQueryable<HocPhi> data) =>
        from hp in data
        join sv in _db.Set<SinhVien>().AsNoTracking() on hp.SinhVienId equals sv.Id
        join hk in _db.Set<HocKy>().AsNoTracking() on hp.HocKyId equals hk.Id
        select new HocPhiDto(hp.Id, sv.Id, sv.MSSV, sv.HoTen, hk.Id, hk.Ten,
            hp.SoTien, hp.SoTienDaNop,
            hp.SoTien > hp.SoTienDaNop ? hp.SoTien - hp.SoTienDaNop : 0m,
            hp.SoTienDaNop > hp.SoTien ? hp.SoTienDaNop - hp.SoTien : 0m,
            hp.DaThanhToan, hp.CreatedAt, hp.UpdatedAt);

    private IQueryable<ThanhToanHocPhiDto> ProjectThanhToan(IQueryable<ThanhToanHocPhi> data) =>
        from tt in data
        join tk in _db.Set<TaiKhoan>().AsNoTracking() on tt.NguoiThuId equals tk.Id into nhom
        from tk in nhom.DefaultIfEmpty()
        select new ThanhToanHocPhiDto(tt.Id, tt.HocPhiId, tt.SoTien, tt.NgayThanhToan,
            tt.HinhThuc, tt.GhiChu, tt.NguoiThuId, tk == null ? null : tk.HoTen);
}
