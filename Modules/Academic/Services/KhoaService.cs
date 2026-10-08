using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>MẪU CRUD: Service dùng AppDbContext trực tiếp, không qua Repository.</summary>
public class KhoaService(AppDbContext db) : IKhoaService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<KhoaDto>> GetPagedAsync(PagedQuery query)
    {
        var q = _db.Set<Khoa>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.Ten.Contains(s));
        }

        var total = await q.CountAsync();
        var items = await q.OrderBy(x => x.Ma)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new KhoaDto(x.Id, x.Ma, x.Ten, x.MoTa, x.CreatedAt, x.UpdatedAt))
            .ToListAsync();

        return new PagedResult<KhoaDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<KhoaDto?> GetByIdAsync(int id)
    {
        var entity = await _db.Set<Khoa>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<KhoaDto> CreateAsync(KhoaRequest request)
    {
        if (await _db.Set<Khoa>().AnyAsync(x => x.Ma == request.Ma))
            throw new InvalidOperationException($"Mã khoa '{request.Ma}' đã tồn tại.");

        var entity = new Khoa { Ma = request.Ma, Ten = request.Ten, MoTa = request.MoTa };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<KhoaDto?> UpdateAsync(int id, KhoaRequest request)
    {
        var entity = await _db.Set<Khoa>().FindAsync(id);
        if (entity is null) return null;

        if (await _db.Set<Khoa>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw new InvalidOperationException($"Mã khoa '{request.Ma}' đã tồn tại.");

        entity.Ma = request.Ma;
        entity.Ten = request.Ten;
        entity.MoTa = request.MoTa;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Set<Khoa>().FindAsync(id);
        if (entity is null) return false;

        // Khóa ngoại dùng Restrict: còn Bộ môn/Ngành tham chiếu thì SQL Server từ chối xóa.
        _db.Remove(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa khoa đang có dữ liệu liên quan.");
        }
        return true;
    }

    private static KhoaDto ToDto(Khoa x) => new(x.Id, x.Ma, x.Ten, x.MoTa, x.CreatedAt, x.UpdatedAt);
}
