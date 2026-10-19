using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ quản lý Chuyên ngành (EDU-02).</summary>
public class ChuyenNganhService(AppDbContext db) : IChuyenNganhService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<ChuyenNganhDto>> GetPagedAsync(ChuyenNganhFilterQuery query)
    {
        var q = _db.Set<ChuyenNganh>().AsNoTracking();

        if (query.NganhId.HasValue)
        {
            q = q.Where(x => x.NganhId == query.NganhId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.Ten.Contains(s));
        }

        var total = await q.CountAsync();

        var queryJoin = from cn in q
                        join ng in _db.Set<Nganh>().AsNoTracking() on cn.NganhId equals ng.Id into ngGroup
                        from nganh in ngGroup.DefaultIfEmpty()
                        orderby cn.Ma
                        select new ChuyenNganhDto(
                            cn.Id,
                            cn.Ma,
                            cn.Ten,
                            cn.NganhId,
                            nganh != null ? nganh.Ten : null,
                            cn.CreatedAt,
                            cn.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<ChuyenNganhDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ChuyenNganhDto?> GetByIdAsync(int id)
    {
        var cn = await _db.Set<ChuyenNganh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (cn is null) return null;

        var nganh = await _db.Set<Nganh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == cn.NganhId);
        return new ChuyenNganhDto(cn.Id, cn.Ma, cn.Ten, cn.NganhId, nganh?.Ten, cn.CreatedAt, cn.UpdatedAt);
    }

    public async Task<ChuyenNganhDto> CreateAsync(ChuyenNganhRequest request)
    {
        if (await _db.Set<ChuyenNganh>().AnyAsync(x => x.Ma == request.Ma))
            throw new InvalidOperationException($"Mã chuyên ngành '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw new InvalidOperationException($"Ngành với Id {request.NganhId} không tồn tại.");

        var cn = new ChuyenNganh
        {
            Ma = request.Ma.Trim(),
            Ten = request.Ten.Trim(),
            NganhId = request.NganhId
        };

        _db.Add(cn);
        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(cn.NganhId);
        return new ChuyenNganhDto(cn.Id, cn.Ma, cn.Ten, cn.NganhId, nganh?.Ten, cn.CreatedAt, cn.UpdatedAt);
    }

    public async Task<ChuyenNganhDto?> UpdateAsync(int id, ChuyenNganhRequest request)
    {
        var cn = await _db.Set<ChuyenNganh>().FindAsync(id);
        if (cn is null) return null;

        if (await _db.Set<ChuyenNganh>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw new InvalidOperationException($"Mã chuyên ngành '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw new InvalidOperationException($"Ngành với Id {request.NganhId} không tồn tại.");

        cn.Ma = request.Ma.Trim();
        cn.Ten = request.Ten.Trim();
        cn.NganhId = request.NganhId;

        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(cn.NganhId);
        return new ChuyenNganhDto(cn.Id, cn.Ma, cn.Ten, cn.NganhId, nganh?.Ten, cn.CreatedAt, cn.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cn = await _db.Set<ChuyenNganh>().FindAsync(id);
        if (cn is null) return false;

        _db.Remove(cn);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa chuyên ngành đang có sinh viên tham gia.");
        }

        return true;
    }
}
