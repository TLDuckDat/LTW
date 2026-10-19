using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Services;

public class PhongHocService(AppDbContext db) : IPhongHocService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<PhongHocDto>> GetPagedAsync(PagedQuery query)
    {
        var q = _db.Set<PhongHoc>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s));
        }

        var total = await q.CountAsync();
        var items = await q.OrderBy(x => x.Ma)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new PhongHocDto(x.Id, x.Ma, x.SucChua, x.CreatedAt, x.UpdatedAt))
            .ToListAsync();

        return new PagedResult<PhongHocDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<PhongHocDto?> GetByIdAsync(int id)
    {
        var entity = await _db.Set<PhongHoc>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<PhongHocDto> CreateAsync(PhongHocRequest request)
    {
        var ma = request.Ma.Trim();
        if (await _db.Set<PhongHoc>().AnyAsync(x => x.Ma == ma))
            throw new InvalidOperationException($"Mã phòng học '{ma}' đã tồn tại.");

        var entity = new PhongHoc { Ma = ma, SucChua = request.SucChua };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<PhongHocDto?> UpdateAsync(int id, PhongHocRequest request)
    {
        var entity = await _db.Set<PhongHoc>().FindAsync(id);
        if (entity is null) return null;

        var ma = request.Ma.Trim();
        if (await _db.Set<PhongHoc>().AnyAsync(x => x.Ma == ma && x.Id != id))
            throw new InvalidOperationException($"Mã phòng học '{ma}' đã tồn tại.");

        // Giảm sức chứa không được làm phòng không đủ chỗ cho lớp đã xếp vào
        var siSoLonNhat = await _db.Set<LichHoc>()
            .Where(l => l.PhongHocId == id)
            .Join(_db.Set<LopHocPhan>(), l => l.LopHocPhanId, lhp => lhp.Id, (l, lhp) => (int?)lhp.SiSoToiDa)
            .MaxAsync();
        if (siSoLonNhat is not null && request.SucChua < siSoLonNhat)
            throw new InvalidOperationException($"Sức chứa nhỏ hơn sĩ số của lớp đã xếp vào phòng ({siSoLonNhat}).");

        entity.Ma = ma;
        entity.SucChua = request.SucChua;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Set<PhongHoc>().FindAsync(id);
        if (entity is null) return false;

        if (await _db.Set<LichHoc>().AnyAsync(x => x.PhongHocId == id))
            throw new InvalidOperationException("Phòng học đang được xếp lịch, không thể xóa.");

        _db.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static PhongHocDto ToDto(PhongHoc x) => new(x.Id, x.Ma, x.SucChua, x.CreatedAt, x.UpdatedAt);
}
