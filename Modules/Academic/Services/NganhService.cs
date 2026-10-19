using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ quản lý Ngành đào tạo (EDU-02).</summary>
public class NganhService(AppDbContext db) : INganhService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<NganhDto>> GetPagedAsync(NganhFilterQuery query)
    {
        var q = _db.Set<Nganh>().AsNoTracking();

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

        var queryJoin = from ng in q
                        join k in _db.Set<Khoa>().AsNoTracking() on ng.KhoaId equals k.Id into kGroup
                        from khoa in kGroup.DefaultIfEmpty()
                        orderby ng.Ma
                        select new NganhDto(
                            ng.Id,
                            ng.Ma,
                            ng.Ten,
                            ng.KhoaId,
                            khoa != null ? khoa.Ten : null,
                            ng.CreatedAt,
                            ng.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<NganhDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<NganhDto> GetByIdAsync(int id)
    {
        var ng = await _db.Set<Nganh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (ng is null) throw AppException.NotFound("ngành đào tạo");

        var khoa = await _db.Set<Khoa>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == ng.KhoaId);
        return new NganhDto(ng.Id, ng.Ma, ng.Ten, ng.KhoaId, khoa?.Ten, ng.CreatedAt, ng.UpdatedAt);
    }

    public async Task<NganhDto> CreateAsync(NganhRequest request)
    {
        if (await _db.Set<Nganh>().AnyAsync(x => x.Ma == request.Ma))
            throw AppException.Conflict($"Mã ngành '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Khoa>().AnyAsync(x => x.Id == request.KhoaId))
            throw AppException.Conflict($"Khoa với Id {request.KhoaId} không tồn tại.");

        var ng = new Nganh
        {
            Ma = request.Ma.Trim(),
            Ten = request.Ten.Trim(),
            KhoaId = request.KhoaId
        };

        _db.Add(ng);
        await _db.SaveChangesAsync();

        var khoa = await _db.Set<Khoa>().FindAsync(ng.KhoaId);
        return new NganhDto(ng.Id, ng.Ma, ng.Ten, ng.KhoaId, khoa?.Ten, ng.CreatedAt, ng.UpdatedAt);
    }

    public async Task<NganhDto> UpdateAsync(int id, NganhRequest request)
    {
        var ng = await _db.Set<Nganh>().FindAsync(id);
        if (ng is null) throw AppException.NotFound("ngành đào tạo");

        if (await _db.Set<Nganh>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw AppException.Conflict($"Mã ngành '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Khoa>().AnyAsync(x => x.Id == request.KhoaId))
            throw AppException.Conflict($"Khoa với Id {request.KhoaId} không tồn tại.");

        ng.Ma = request.Ma.Trim();
        ng.Ten = request.Ten.Trim();
        ng.KhoaId = request.KhoaId;

        await _db.SaveChangesAsync();

        var khoa = await _db.Set<Khoa>().FindAsync(ng.KhoaId);
        return new NganhDto(ng.Id, ng.Ma, ng.Ten, ng.KhoaId, khoa?.Ten, ng.CreatedAt, ng.UpdatedAt);
    }

    public async Task DeleteAsync(int id)
    {
        var ng = await _db.Set<Nganh>().FindAsync(id);
        if (ng is null) throw AppException.NotFound("ngành đào tạo");

        _db.Remove(ng);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa ngành đang có chuyên ngành, lớp hành chính hoặc sinh viên liên quan.");
        }
    }
}
