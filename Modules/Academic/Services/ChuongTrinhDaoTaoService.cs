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
    public async Task<ChuongTrinhDaoTaoDetailDto?> GetByIdAsync(int id)
    {
        var ctdt = await _db.Set<ChuongTrinhDaoTao>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (ctdt is null) return null;

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
            danhSachMonHoc,
            ctdt.CreatedAt,
            ctdt.UpdatedAt
        );
    }

    // EDU-04: Tạo mới CTĐT
    public async Task<ChuongTrinhDaoTaoDto> CreateAsync(ChuongTrinhDaoTaoRequest request)
    {
        if (await _db.Set<ChuongTrinhDaoTao>().AnyAsync(x => x.Ma == request.Ma))
            throw new InvalidOperationException($"Mã chương trình đào tạo '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw new InvalidOperationException($"Ngành với Id {request.NganhId} không tồn tại.");

        var ctdt = new ChuongTrinhDaoTao
        {
            Ma = request.Ma.Trim(),
            Ten = request.Ten.Trim(),
            NganhId = request.NganhId,
            KhoaTuyen = request.KhoaTuyen,
            TongSoTinChi = request.TongSoTinChi
        };

        _db.Add(ctdt);
        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(ctdt.NganhId);
        return new ChuongTrinhDaoTaoDto(
            ctdt.Id, ctdt.Ma, ctdt.Ten, ctdt.NganhId, nganh?.Ten,
            ctdt.KhoaTuyen, ctdt.TongSoTinChi, 0, ctdt.CreatedAt, ctdt.UpdatedAt
        );
    }

    // EDU-04: Cập nhật CTĐT
    public async Task<ChuongTrinhDaoTaoDto?> UpdateAsync(int id, ChuongTrinhDaoTaoRequest request)
    {
        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(id);
        if (ctdt is null) return null;

        if (await _db.Set<ChuongTrinhDaoTao>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw new InvalidOperationException($"Mã chương trình đào tạo '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw new InvalidOperationException($"Ngành với Id {request.NganhId} không tồn tại.");

        ctdt.Ma = request.Ma.Trim();
        ctdt.Ten = request.Ten.Trim();
        ctdt.NganhId = request.NganhId;
        ctdt.KhoaTuyen = request.KhoaTuyen;
        ctdt.TongSoTinChi = request.TongSoTinChi;

        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(ctdt.NganhId);
        var soLuongMon = await _db.Set<ChuongTrinhDaoTaoMonHoc>().CountAsync(m => m.ChuongTrinhDaoTaoId == ctdt.Id);
        return new ChuongTrinhDaoTaoDto(
            ctdt.Id, ctdt.Ma, ctdt.Ten, ctdt.NganhId, nganh?.Ten,
            ctdt.KhoaTuyen, ctdt.TongSoTinChi, soLuongMon, ctdt.CreatedAt, ctdt.UpdatedAt
        );
    }

    // EDU-04: Xóa CTĐT
    public async Task<bool> DeleteAsync(int id)
    {
        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(id);
        if (ctdt is null) return false;

        _db.Remove(ctdt);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa chương trình đào tạo đang có dữ liệu sinh viên hoặc đăng ký liên quan.");
        }

        return true;
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
            throw new InvalidOperationException($"Chương trình đào tạo với Id {chuongTrinhDaoTaoId} không tồn tại.");

        var monHoc = await _db.Set<MonHoc>().FindAsync(request.MonHocId);
        if (monHoc is null)
            throw new InvalidOperationException($"Môn học với Id {request.MonHocId} không tồn tại.");

        if (await _db.Set<ChuongTrinhDaoTaoMonHoc>().AnyAsync(x => x.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId && x.MonHocId == request.MonHocId))
            throw new InvalidOperationException($"Môn học '{monHoc.Ten}' đã có trong chương trình đào tạo này.");

        var item = new ChuongTrinhDaoTaoMonHoc
        {
            ChuongTrinhDaoTaoId = chuongTrinhDaoTaoId,
            MonHocId = request.MonHocId,
            HocKyGoiY = request.HocKyGoiY,
            BatBuoc = request.BatBuoc,
            GhiChu = request.GhiChu?.Trim()
        };

        _db.Add(item);

        // Tự động cập nhật tổng số tín chỉ nếu môn này là bắt buộc và CTĐT chưa tính
        if (request.BatBuoc)
        {
            ctdt.TongSoTinChi += monHoc.SoTinChi;
        }

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
    public async Task<MonHocTrongCtdtDto?> CapNhatMonHocAsync(int chuongTrinhDaoTaoId, int monHocId, CapNhatMonHocTrongCtdtRequest request)
    {
        var item = await _db.Set<ChuongTrinhDaoTaoMonHoc>()
            .FirstOrDefaultAsync(x => x.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId && x.MonHocId == monHocId);
        if (item is null) return null;

        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(chuongTrinhDaoTaoId);
        var monHoc = await _db.Set<MonHoc>().FindAsync(monHocId);

        // Nếu thay đổi trạng thái bắt buộc <-> tự chọn, điều chỉnh lại tổng số tín chỉ bắt buộc
        if (ctdt != null && monHoc != null && item.BatBuoc != request.BatBuoc)
        {
            if (request.BatBuoc)
            {
                ctdt.TongSoTinChi += monHoc.SoTinChi;
            }
            else
            {
                ctdt.TongSoTinChi = Math.Max(0, ctdt.TongSoTinChi - monHoc.SoTinChi);
            }
        }

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
    public async Task<bool> XoaMonHocAsync(int chuongTrinhDaoTaoId, int monHocId)
    {
        var item = await _db.Set<ChuongTrinhDaoTaoMonHoc>()
            .FirstOrDefaultAsync(x => x.ChuongTrinhDaoTaoId == chuongTrinhDaoTaoId && x.MonHocId == monHocId);
        if (item is null) return false;

        var ctdt = await _db.Set<ChuongTrinhDaoTao>().FindAsync(chuongTrinhDaoTaoId);
        var monHoc = await _db.Set<MonHoc>().FindAsync(monHocId);

        if (ctdt != null && monHoc != null && item.BatBuoc)
        {
            ctdt.TongSoTinChi = Math.Max(0, ctdt.TongSoTinChi - monHoc.SoTinChi);
        }

        _db.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}
