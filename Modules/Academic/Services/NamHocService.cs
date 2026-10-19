using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ quản lý Năm học (EDU-03).</summary>
public class NamHocService(AppDbContext db) : INamHocService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<NamHocDto>> GetPagedAsync(PagedQuery query)
    {
        var q = _db.Set<NamHoc>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ten.Contains(s));
        }

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.NgayBatDau)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new NamHocDto(x.Id, x.Ten, x.NgayBatDau, x.NgayKetThuc, x.CreatedAt, x.UpdatedAt))
            .ToListAsync();

        return new PagedResult<NamHocDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<NamHocDto> GetByIdAsync(int id)
    {
        var nh = await _db.Set<NamHoc>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return nh is null ? throw AppException.NotFound("năm học") : new NamHocDto(nh.Id, nh.Ten, nh.NgayBatDau, nh.NgayKetThuc, nh.CreatedAt, nh.UpdatedAt);
    }

    public async Task<NamHocDto> CreateAsync(NamHocRequest request)
    {
        if (request.NgayKetThuc <= request.NgayBatDau)
            throw AppException.Conflict("Ngày kết thúc năm học phải sau ngày bắt đầu.");

        if (await _db.Set<NamHoc>().AnyAsync(x => x.Ten == request.Ten))
            throw AppException.Conflict($"Năm học '{request.Ten}' đã tồn tại.");

        var nh = new NamHoc
        {
            Ten = request.Ten.Trim(),
            NgayBatDau = request.NgayBatDau,
            NgayKetThuc = request.NgayKetThuc
        };

        _db.Add(nh);
        await _db.SaveChangesAsync();

        return new NamHocDto(nh.Id, nh.Ten, nh.NgayBatDau, nh.NgayKetThuc, nh.CreatedAt, nh.UpdatedAt);
    }

    public async Task<NamHocDto> UpdateAsync(int id, NamHocRequest request)
    {
        var nh = await _db.Set<NamHoc>().FindAsync(id);
        if (nh is null) throw AppException.NotFound("năm học");

        if (request.NgayKetThuc <= request.NgayBatDau)
            throw AppException.Conflict("Ngày kết thúc năm học phải sau ngày bắt đầu.");

        if (await _db.Set<NamHoc>().AnyAsync(x => x.Ten == request.Ten && x.Id != id))
            throw AppException.Conflict($"Năm học '{request.Ten}' đã tồn tại.");

        nh.Ten = request.Ten.Trim();
        nh.NgayBatDau = request.NgayBatDau;
        nh.NgayKetThuc = request.NgayKetThuc;

        await _db.SaveChangesAsync();

        return new NamHocDto(nh.Id, nh.Ten, nh.NgayBatDau, nh.NgayKetThuc, nh.CreatedAt, nh.UpdatedAt);
    }

    public async Task DeleteAsync(int id)
    {
        var nh = await _db.Set<NamHoc>().FindAsync(id);
        if (nh is null) throw AppException.NotFound("năm học");

        _db.Remove(nh);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa năm học đang có học kỳ liên quan.");
        }
    }
}
