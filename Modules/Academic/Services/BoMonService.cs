using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ quản lý Bộ môn (EDU-01).</summary>
public class BoMonService(AppDbContext db) : IBoMonService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<BoMonDto>> GetPagedAsync(BoMonFilterQuery query)
    {
        var q = _db.Set<BoMon>().AsNoTracking();

        if (query.KhoaId.HasValue)
        {
            q = q.Where(x => x.KhoaId == query.KhoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.Ten.Contains(s));
        }

        var total = await q.CountAsync();

        var queryJoin = from bm in q
                        join k in _db.Set<Khoa>().AsNoTracking() on bm.KhoaId equals k.Id into kGroup
                        from khoa in kGroup.DefaultIfEmpty()
                        orderby bm.Ma
                        select new BoMonDto(
                            bm.Id,
                            bm.Ma,
                            bm.Ten,
                            bm.KhoaId,
                            khoa != null ? khoa.Ten : null,
                            bm.CreatedAt,
                            bm.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<BoMonDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<BoMonDto?> GetByIdAsync(int id)
    {
        var bm = await _db.Set<BoMon>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (bm is null) return null;

        var khoa = await _db.Set<Khoa>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == bm.KhoaId);
        return new BoMonDto(bm.Id, bm.Ma, bm.Ten, bm.KhoaId, khoa?.Ten, bm.CreatedAt, bm.UpdatedAt);
    }

    public async Task<BoMonDto> CreateAsync(BoMonRequest request)
    {
        if (await _db.Set<BoMon>().AnyAsync(x => x.Ma == request.Ma))
            throw new InvalidOperationException($"Mã bộ môn '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Khoa>().AnyAsync(x => x.Id == request.KhoaId))
            throw new InvalidOperationException($"Khoa với Id {request.KhoaId} không tồn tại.");

        var bm = new BoMon
        {
            Ma = request.Ma.Trim(),
            Ten = request.Ten.Trim(),
            KhoaId = request.KhoaId
        };

        _db.Add(bm);
        await _db.SaveChangesAsync();

        var khoa = await _db.Set<Khoa>().FindAsync(bm.KhoaId);
        return new BoMonDto(bm.Id, bm.Ma, bm.Ten, bm.KhoaId, khoa?.Ten, bm.CreatedAt, bm.UpdatedAt);
    }

    public async Task<BoMonDto?> UpdateAsync(int id, BoMonRequest request)
    {
        var bm = await _db.Set<BoMon>().FindAsync(id);
        if (bm is null) return null;

        if (await _db.Set<BoMon>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw new InvalidOperationException($"Mã bộ môn '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Khoa>().AnyAsync(x => x.Id == request.KhoaId))
            throw new InvalidOperationException($"Khoa với Id {request.KhoaId} không tồn tại.");

        bm.Ma = request.Ma.Trim();
        bm.Ten = request.Ten.Trim();
        bm.KhoaId = request.KhoaId;

        await _db.SaveChangesAsync();

        var khoa = await _db.Set<Khoa>().FindAsync(bm.KhoaId);
        return new BoMonDto(bm.Id, bm.Ma, bm.Ten, bm.KhoaId, khoa?.Ten, bm.CreatedAt, bm.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var bm = await _db.Set<BoMon>().FindAsync(id);
        if (bm is null) return false;

        _db.Remove(bm);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa bộ môn đang có giảng viên hoặc dữ liệu liên quan.");
        }

        return true;
    }
}
