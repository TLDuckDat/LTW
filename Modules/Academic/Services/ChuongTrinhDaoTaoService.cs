using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ quản lý Chương trình đào tạo (EDU-04) và Môn học trong CTĐT (EDU-05).</summary>
public class ChuongTrinhDaoTaoService(AppDbContext db) : IChuongTrinhDaoTaoService
{
    private readonly AppDbContext _db = db;

    // EDU-04: Danh sách CTĐT phân trang
    public async Task<PagedResult<ChuongTrinhDaoTaoDto>> GetPagedAsync(ChuongTrinhDaoTaoFilterQuery query)
    {
        var q = _db.Set<ChuongTrinhDaoTao>().AsNoTracking();

        if (query.NganhId.HasValue)
        {
            q = q.Where(x => x.NganhId == query.NganhId.Value);
        }

        if (query.KhoaTuyen.HasValue)
        {
            q = q.Where(x => x.KhoaTuyen == query.KhoaTuyen.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.Ten.Contains(s));
        }

        var total = await q.CountAsync();

        var queryJoin = from ctdt in q
                        join ng in _db.Set<Nganh>().AsNoTracking() on ctdt.NganhId equals ng.Id into ngGroup
                        from nganh in ngGroup.DefaultIfEmpty()
                        orderby ctdt.KhoaTuyen descending, ctdt.Ma
                        select new ChuongTrinhDaoTaoDto(
                            ctdt.Id,
                            ctdt.Ma,
                            ctdt.Ten,
                            ctdt.NganhId,
                            nganh != null ? nganh.Ten : null,
                            ctdt.KhoaTuyen,
                            ctdt.TongSoTinChi,
                            ctdt.CpaToiThieu,
                            ctdt.YeuCauThucTap,
                            ctdt.DiemThucTapToiThieu,
                            ctdt.YeuCauKhoaLuan,
                            ctdt.DiemBaoVeToiThieu,
                            _db.Set<ChuongTrinhDaoTaoMonHoc>().Count(m => m.ChuongTrinhDaoTaoId == ctdt.Id),
                            ctdt.CreatedAt,
                            ctdt.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<ChuongTrinhDaoTaoDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    // EDU-04: Chi tiết CTĐT kèm danh sách môn học
    public async Task<ChuongTrinhDaoTaoDetailDto> GetByIdAsync(int id)
    {
        var ctdt = await _db.Set<ChuongTrinhDaoTao>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (ctdt is null) throw AppException.NotFound("chương trình đào tạo");

        var nganh = await _db.Set<Nganh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ctdt.NganhId);

        var danhSachMonHoc = await GetDanhSachMonHocAsync(id);

        return new ChuongTrinhDaoTaoDetailDto(
            ctdt.Id,
            ctdt.Ma,
            ctdt.Ten,
            ctdt.NganhId,
            nganh?.Ten,
            ctdt.KhoaTuyen,
            ctdt.TongSoTinChi,
            danhSachMonHoc.Where(x => x.BatBuoc).Sum(x => x.SoTinChi ?? 0),
            ctdt.CpaToiThieu,
            ctdt.YeuCauThucTap,
            ctdt.DiemThucTapToiThieu,
            ctdt.YeuCauKhoaLuan,
            ctdt.DiemBaoVeToiThieu,
            danhSachMonHoc,
            ctdt.CreatedAt,
            ctdt.UpdatedAt
        );
    }

    // EDU-04: Tạo mới CTĐT
    public async Task<ChuongTrinhDaoTaoDto> CreateAsync(ChuongTrinhDaoTaoRequest request)
    {
        if (await _db.Set<ChuongTrinhDaoTao>().AnyAsync(x => x.Ma == request.Ma))
            throw AppException.Conflict($"Mã chương trình đào tạo '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw AppException.Conflict($"Ngành với Id {request.NganhId} không tồn tại.");
        KiemTraDieuKienTotNghiep(request);

        var ctdt = new ChuongTrinhDaoTao
        {
            Ma = request.Ma.Trim(),
            Ten = request.Ten.Trim(),
            NganhId = request.NganhId,
            KhoaTuyen = request.KhoaTuyen
        };
        GanThongTin(ctdt, request);

        _db.Add(ctdt);
        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(ctdt.NganhId);
        return ToDto(ctdt, nganh?.Ten, 0);
    }

    // EDU-04: Cập nhật CTĐT
    public async Task<ChuongTrinhDaoTaoDto> UpdateAsync(int id, ChuongTrinhDaoTaoRequest request)
    {
        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(id);
        if (ctdt is null) throw AppException.NotFound("chương trình đào tạo");

        if (await _db.Set<ChuongTrinhDaoTao>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw AppException.Conflict($"Mã chương trình đào tạo '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw AppException.Conflict($"Ngành với Id {request.NganhId} không tồn tại.");
        KiemTraDieuKienTotNghiep(request);

        var tinChiBatBuoc = await TongTinChiBatBuocAsync(id);
        if (request.TongSoTinChi > 0 && request.TongSoTinChi < tinChiBatBuoc)
            throw AppException.Conflict(
                $"Tổng số tín chỉ ({request.TongSoTinChi}) không được nhỏ hơn tổng tín chỉ các môn bắt buộc ({tinChiBatBuoc}).");

        ctdt.Ma = request.Ma.Trim();
        ctdt.Ten = request.Ten.Trim();
        ctdt.NganhId = request.NganhId;
        ctdt.KhoaTuyen = request.KhoaTuyen;
        GanThongTin(ctdt, request);

        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(ctdt.NganhId);
        var soLuongMon = await _db.Set<ChuongTrinhDaoTaoMonHoc>().CountAsync(m => m.ChuongTrinhDaoTaoId == ctdt.Id);
        return ToDto(ctdt, nganh?.Ten, soLuongMon);
    }

    // Bắt buộc thực tập/khóa luận thì phải có điểm tối thiểu, nếu không Graduation không xét được
    private static void KiemTraDieuKienTotNghiep(ChuongTrinhDaoTaoRequest request)
    {
        if (request.YeuCauThucTap == true && request.DiemThucTapToiThieu is null)
            throw AppException.Conflict("Chương trình bắt buộc thực tập phải có điểm thực tập tối thiểu.");
        if (request.YeuCauKhoaLuan == true && request.DiemBaoVeToiThieu is null)
            throw AppException.Conflict("Chương trình bắt buộc khóa luận phải có điểm bảo vệ tối thiểu.");
        if (new[] { request.CpaToiThieu, request.DiemThucTapToiThieu, request.DiemBaoVeToiThieu }
            .Any(x => x is not null && decimal.Round(x.Value, 2) != x.Value))
            throw AppException.Conflict("CPA và điểm tối thiểu chỉ được có tối đa 2 chữ số thập phân.");
    }

    private static void GanThongTin(ChuongTrinhDaoTao ctdt, ChuongTrinhDaoTaoRequest request)
    {
        ctdt.TongSoTinChi = request.TongSoTinChi;
        ctdt.CpaToiThieu = request.CpaToiThieu;
        ctdt.YeuCauThucTap = request.YeuCauThucTap;
        ctdt.DiemThucTapToiThieu = request.DiemThucTapToiThieu;
        ctdt.YeuCauKhoaLuan = request.YeuCauKhoaLuan;
        ctdt.DiemBaoVeToiThieu = request.DiemBaoVeToiThieu;
    }

    private static ChuongTrinhDaoTaoDto ToDto(ChuongTrinhDaoTao ctdt, string? tenNganh, int soLuongMon) => new(
        ctdt.Id, ctdt.Ma, ctdt.Ten, ctdt.NganhId, tenNganh, ctdt.KhoaTuyen, ctdt.TongSoTinChi,
        ctdt.CpaToiThieu, ctdt.YeuCauThucTap, ctdt.DiemThucTapToiThieu, ctdt.YeuCauKhoaLuan, ctdt.DiemBaoVeToiThieu,
        soLuongMon, ctdt.CreatedAt, ctdt.UpdatedAt);

    private Task<int> TongTinChiBatBuocAsync(int chuongTrinhDaoTaoId, int? boQuaMonHocId = null)
        => (from ctm in _db.Set<ChuongTrinhDaoTaoMonHoc>()
            join mh in _db.Set<MonHoc>() on ctm.MonHocId equals mh.Id
            where ctm.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId && ctm.BatBuoc
                  && (boQuaMonHocId == null || ctm.MonHocId != boQuaMonHocId)
            select mh.SoTinChi).SumAsync();

    // TongSoTinChi do người dùng nhập (gồm cả tự chọn); chỉ chặn khi môn bắt buộc vượt quá con số này
    private async Task KiemTraVuotTongTinChiAsync(ChuongTrinhDaoTao ctdt, MonHoc monHoc)
    {
        if (ctdt.TongSoTinChi <= 0) return;
        var tong = await TongTinChiBatBuocAsync(ctdt.Id, monHoc.Id) + monHoc.SoTinChi;
        if (tong > ctdt.TongSoTinChi)
            throw AppException.Conflict(
                $"Tổng tín chỉ môn bắt buộc ({tong}) vượt tổng số tín chỉ của chương trình ({ctdt.TongSoTinChi}).");
    }

    // EDU-04: Xóa CTĐT
    public async Task DeleteAsync(int id)
    {
        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(id);
        if (ctdt is null) throw AppException.NotFound("chương trình đào tạo");

        _db.Remove(ctdt);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa chương trình đào tạo đang có dữ liệu sinh viên hoặc đăng ký liên quan.");
        }
    }

    // ==========================================
    // EDU-05: QUẢN LÝ DANH SÁCH MÔN HỌC TRONG CTĐT
    // ==========================================

    // Lấy danh sách môn học của CTĐT (có thể lọc theo kỳ)
    public async Task<List<MonHocTrongCtdtDto>> GetDanhSachMonHocAsync(int chuongTrinhDaoTaoId, int? hocKyGoiY = null)
    {
        var q = _db.Set<ChuongTrinhDaoTaoMonHoc>()
            .AsNoTracking()
            .Where(x => x.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId);

        if (hocKyGoiY.HasValue)
        {
            q = q.Where(x => x.HocKyGoiY == hocKyGoiY.Value);
        }

        var queryJoin = from ctm in q
                        join mh in _db.Set<MonHoc>().AsNoTracking() on ctm.MonHocId equals mh.Id into mhGroup
                        from mon in mhGroup.DefaultIfEmpty()
                        orderby ctm.HocKyGoiY, ctm.BatBuoc descending, mon != null ? mon.Ma : ""
                        select new MonHocTrongCtdtDto(
                            ctm.Id,
                            ctm.ChuongTrinhDaoTaoId,
                            ctm.MonHocId,
                            mon != null ? mon.Ma : null,
                            mon != null ? mon.Ten : null,
                            mon != null ? mon.SoTinChi : null,
                            ctm.HocKyGoiY,
                            ctm.BatBuoc,
                            ctm.GhiChu,
                            ctm.CreatedAt
                        );

        return await queryJoin.ToListAsync();
    }

    // Thêm môn học vào CTĐT
    public async Task<MonHocTrongCtdtDto> ThemMonHocAsync(int chuongTrinhDaoTaoId, ThemMonHocVaoCtdtRequest request)
    {
        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(chuongTrinhDaoTaoId);
        if (ctdt is null)
            throw AppException.Conflict($"Chương trình đào tạo với Id {chuongTrinhDaoTaoId} không tồn tại.");

        var monHoc = await _db.Set<MonHoc>().FindAsync(request.MonHocId);
        if (monHoc is null)
            throw AppException.Conflict($"Môn học với Id {request.MonHocId} không tồn tại.");

        if (await _db.Set<ChuongTrinhDaoTaoMonHoc>().AnyAsync(x => x.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId && x.MonHocId == request.MonHocId))
            throw AppException.Conflict($"Môn học '{monHoc.Ten}' đã có trong chương trình đào tạo này.");

        var item = new ChuongTrinhDaoTaoMonHoc
        {
            ChuongTrinhDaoTaoId = chuongTrinhDaoTaoId,
            MonHocId = request.MonHocId,
            HocKyGoiY = request.HocKyGoiY,
            BatBuoc = request.BatBuoc,
            GhiChu = request.GhiChu?.Trim()
        };

        if (request.BatBuoc)
            await KiemTraVuotTongTinChiAsync(ctdt, monHoc);

        _db.Add(item);
        await _db.SaveChangesAsync();

        return new MonHocTrongCtdtDto(
            item.Id,
            item.ChuongTrinhDaoTaoId,
            item.MonHocId,
            monHoc.Ma,
            monHoc.Ten,
            monHoc.SoTinChi,
            item.HocKyGoiY,
            item.BatBuoc,
            item.GhiChu,
            item.CreatedAt
        );
    }

    // Cập nhật thông tin môn học trong CTĐT
    public async Task<MonHocTrongCtdtDto> CapNhatMonHocAsync(int chuongTrinhDaoTaoId, int monHocId, CapNhatMonHocTrongCtdtRequest request)
    {
        var item = await _db.Set<ChuongTrinhDaoTaoMonHoc>()
            .FirstOrDefaultAsync(x => x.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId && x.MonHocId == monHocId);
        if (item is null) throw AppException.NotFound("môn học trong chương trình đào tạo này");

        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(chuongTrinhDaoTaoId);
        var monHoc = await _db.Set<MonHoc>().FindAsync(monHocId);

        if (ctdt != null && monHoc != null && request.BatBuoc && !item.BatBuoc)
            await KiemTraVuotTongTinChiAsync(ctdt, monHoc);

        item.HocKyGoiY = request.HocKyGoiY;
        item.BatBuoc = request.BatBuoc;
        item.GhiChu = request.GhiChu?.Trim();

        await _db.SaveChangesAsync();

        return new MonHocTrongCtdtDto(
            item.Id,
            item.ChuongTrinhDaoTaoId,
            item.MonHocId,
            monHoc?.Ma,
            monHoc?.Ten,
            monHoc?.SoTinChi,
            item.HocKyGoiY,
            item.BatBuoc,
            item.GhiChu,
            item.CreatedAt
        );
    }

    // Gỡ môn học khỏi CTĐT
    public async Task XoaMonHocAsync(int chuongTrinhDaoTaoId, int monHocId)
    {
        var item = await _db.Set<ChuongTrinhDaoTaoMonHoc>()
            .FirstOrDefaultAsync(x => x.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId && x.MonHocId == monHocId);
        if (item is null) throw AppException.NotFound("môn học trong chương trình đào tạo này");

        _db.Remove(item);
        await _db.SaveChangesAsync();
    }
}
