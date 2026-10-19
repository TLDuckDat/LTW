using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ quản lý Quy chế và điều kiện học vụ (EDU-06).</summary>
public class QuyCheHocVuService(AppDbContext db) : IQuyCheHocVuService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<QuyCheHocVuDto>> GetPagedAsync(QuyCheHocVuFilterQuery query)
    {
        var q = _db.Set<QuyCheHocVu>().AsNoTracking();

        if (query.ApDungTuKhoa.HasValue)
        {
            q = q.Where(x => x.ApDungTuKhoa == query.ApDungTuKhoa.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.Ten.Contains(s) || (x.MoTa != null && x.MoTa.Contains(s)));
        }

        var total = await q.CountAsync();
        var items = await q.OrderBy(x => x.Ma)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new QuyCheHocVuDto(x.Id, x.Ma, x.Ten, x.GiaTri, x.DonVi, x.MoTa, x.ApDungTuKhoa, x.CreatedAt, x.UpdatedAt))
            .ToListAsync();

        return new PagedResult<QuyCheHocVuDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<QuyCheHocVuDto?> GetByIdAsync(int id)
    {
        var qc = await _db.Set<QuyCheHocVu>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return qc is null ? null : new QuyCheHocVuDto(qc.Id, qc.Ma, qc.Ten, qc.GiaTri, qc.DonVi, qc.MoTa, qc.ApDungTuKhoa, qc.CreatedAt, qc.UpdatedAt);
    }

    public async Task<QuyCheHocVuDto?> GetByMaAsync(string ma)
    {
        var qc = await _db.Set<QuyCheHocVu>().AsNoTracking().FirstOrDefaultAsync(x => x.Ma == ma);
        return qc is null ? null : new QuyCheHocVuDto(qc.Id, qc.Ma, qc.Ten, qc.GiaTri, qc.DonVi, qc.MoTa, qc.ApDungTuKhoa, qc.CreatedAt, qc.UpdatedAt);
    }

    public async Task<QuyCheHocVuDto> CreateAsync(QuyCheHocVuRequest request)
    {
        if (await _db.Set<QuyCheHocVu>().AnyAsync(x => x.Ma == request.Ma))
            throw new InvalidOperationException($"Mã quy chế '{request.Ma}' đã tồn tại.");

        var qc = new QuyCheHocVu
        {
            Ma = request.Ma.Trim(),
            Ten = request.Ten.Trim(),
            GiaTri = request.GiaTri.Trim(),
            DonVi = request.DonVi?.Trim(),
            MoTa = request.MoTa?.Trim(),
            ApDungTuKhoa = request.ApDungTuKhoa
        };

        _db.Add(qc);
        await _db.SaveChangesAsync();

        return new QuyCheHocVuDto(qc.Id, qc.Ma, qc.Ten, qc.GiaTri, qc.DonVi, qc.MoTa, qc.ApDungTuKhoa, qc.CreatedAt, qc.UpdatedAt);
    }

    public async Task<QuyCheHocVuDto?> UpdateAsync(int id, QuyCheHocVuRequest request)
    {
        var qc = await _db.Set<QuyCheHocVu>().FindAsync(id);
        if (qc is null) return null;

        if (await _db.Set<QuyCheHocVu>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw new InvalidOperationException($"Mã quy chế '{request.Ma}' đã tồn tại.");

        qc.Ma = request.Ma.Trim();
        qc.Ten = request.Ten.Trim();
        qc.GiaTri = request.GiaTri.Trim();
        qc.DonVi = request.DonVi?.Trim();
        qc.MoTa = request.MoTa?.Trim();
        qc.ApDungTuKhoa = request.ApDungTuKhoa;

        await _db.SaveChangesAsync();

        return new QuyCheHocVuDto(qc.Id, qc.Ma, qc.Ten, qc.GiaTri, qc.DonVi, qc.MoTa, qc.ApDungTuKhoa, qc.CreatedAt, qc.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var qc = await _db.Set<QuyCheHocVu>().FindAsync(id);
        if (qc is null) return false;

        _db.Remove(qc);
        await _db.SaveChangesAsync();
        return true;
    }
}
