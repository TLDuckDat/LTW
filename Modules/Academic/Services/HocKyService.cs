using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ quản lý Học kỳ (EDU-03).</summary>
public class HocKyService(AppDbContext db) : IHocKyService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<HocKyDto>> GetPagedAsync(HocKyFilterQuery query)
    {
        var q = _db.Set<HocKy>().AsNoTracking();

        if (query.NamHocId.HasValue)
        {
            q = q.Where(x => x.NamHocId == query.NamHocId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ten.Contains(s));
        }

        var total = await q.CountAsync();

        var queryJoin = from hk in q
                        join nh in _db.Set<NamHoc>().AsNoTracking() on hk.NamHocId equals nh.Id into nhGroup
                        from namHoc in nhGroup.DefaultIfEmpty()
                        orderby hk.NamHocId descending, hk.SoThuTu
                        select new HocKyDto(
                            hk.Id,
                            hk.Ten,
                            hk.SoThuTu,
                            hk.NamHocId,
                            namHoc != null ? namHoc.Ten : null,
                            hk.NgayBatDau,
                            hk.NgayKetThuc,
                            hk.CreatedAt,
                            hk.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<HocKyDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<HocKyDto> GetByIdAsync(int id)
    {
        var hk = await _db.Set<HocKy>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (hk is null) throw AppException.NotFound("học kỳ");

        var namHoc = await _db.Set<NamHoc>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == hk.NamHocId);
        return new HocKyDto(hk.Id, hk.Ten, hk.SoThuTu, hk.NamHocId, namHoc?.Ten, hk.NgayBatDau, hk.NgayKetThuc, hk.CreatedAt, hk.UpdatedAt);
    }

    public async Task<HocKyDto> CreateAsync(HocKyRequest request)
    {
        if (request.NgayKetThuc <= request.NgayBatDau)
            throw AppException.Conflict("Ngày kết thúc học kỳ phải sau ngày bắt đầu.");

        if (!await _db.Set<NamHoc>().AnyAsync(x => x.Id == request.NamHocId))
            throw AppException.Conflict($"Năm học với Id {request.NamHocId} không tồn tại.");

        if (await _db.Set<HocKy>().AnyAsync(x => x.NamHocId == request.NamHocId && x.SoThuTu == request.SoThuTu))
            throw AppException.Conflict($"Học kỳ số {request.SoThuTu} đã tồn tại trong năm học này.");

        var hk = new HocKy
        {
            Ten = request.Ten.Trim(),
            SoThuTu = request.SoThuTu,
            NamHocId = request.NamHocId,
            NgayBatDau = request.NgayBatDau,
            NgayKetThuc = request.NgayKetThuc
        };

        _db.Add(hk);
        await _db.SaveChangesAsync();

        var namHoc = await _db.Set<NamHoc>().FindAsync(hk.NamHocId);
        return new HocKyDto(hk.Id, hk.Ten, hk.SoThuTu, hk.NamHocId, namHoc?.Ten, hk.NgayBatDau, hk.NgayKetThuc, hk.CreatedAt, hk.UpdatedAt);
    }

    public async Task<HocKyDto> UpdateAsync(int id, HocKyRequest request)
    {
        var hk = await _db.Set<HocKy>().FindAsync(id);
        if (hk is null) throw AppException.NotFound("học kỳ");

        if (request.NgayKetThuc <= request.NgayBatDau)
            throw AppException.Conflict("Ngày kết thúc học kỳ phải sau ngày bắt đầu.");

        if (!await _db.Set<NamHoc>().AnyAsync(x => x.Id == request.NamHocId))
            throw AppException.Conflict($"Năm học với Id {request.NamHocId} không tồn tại.");

        if (await _db.Set<HocKy>().AnyAsync(x => x.NamHocId == request.NamHocId && x.SoThuTu == request.SoThuTu && x.Id != id))
            throw AppException.Conflict($"Học kỳ số {request.SoThuTu} đã tồn tại trong năm học này.");

        hk.Ten = request.Ten.Trim();
        hk.SoThuTu = request.SoThuTu;
        hk.NamHocId = request.NamHocId;
        hk.NgayBatDau = request.NgayBatDau;
        hk.NgayKetThuc = request.NgayKetThuc;

        await _db.SaveChangesAsync();

        var namHoc = await _db.Set<NamHoc>().FindAsync(hk.NamHocId);
        return new HocKyDto(hk.Id, hk.Ten, hk.SoThuTu, hk.NamHocId, namHoc?.Ten, hk.NgayBatDau, hk.NgayKetThuc, hk.CreatedAt, hk.UpdatedAt);
    }

    public async Task DeleteAsync(int id)
    {
        var hk = await _db.Set<HocKy>().FindAsync(id);
        if (hk is null) throw AppException.NotFound("học kỳ");

        _db.Remove(hk);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa học kỳ đang có đợt đăng ký, lớp học phần hoặc điểm liên quan.");
        }
    }
}
