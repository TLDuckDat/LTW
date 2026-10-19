using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Services;

public class GiangVienService(AppDbContext db) : IGiangVienService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<GiangVienDto>> GetPagedAsync(GiangVienFilterQuery query)
    {
        var q = _db.Set<GiangVien>().AsNoTracking();

        if (query.BoMonId.HasValue)
        {
            q = q.Where(x => x.BoMonId == query.BoMonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.HoTen.Contains(s));
        }

        var total = await q.CountAsync();

        var queryJoin = from gv in q
                        join bm in _db.Set<BoMon>().AsNoTracking() on gv.BoMonId equals bm.Id into bmGroup
                        from b in bmGroup.DefaultIfEmpty()
                        orderby gv.Ma
                        select new GiangVienDto(
                            gv.Id,
                            gv.Ma,
                            gv.HoTen,
                            gv.HocVi,
                            gv.Email,
                            gv.SoDienThoai,
                            gv.BoMonId,
                            b != null ? b.Ten : null,
                            gv.CreatedAt,
                            gv.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<GiangVienDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<GiangVienDto> GetByIdAsync(int id)
    {
        var gv = await _db.Set<GiangVien>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (gv is null) throw AppException.NotFound("giảng viên");

        var boMon = await _db.Set<BoMon>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == gv.BoMonId);

        return new GiangVienDto(
            gv.Id,
            gv.Ma,
            gv.HoTen,
            gv.HocVi,
            gv.Email,
            gv.SoDienThoai,
            gv.BoMonId,
            boMon?.Ten,
            gv.CreatedAt,
            gv.UpdatedAt
        );
    }

    public async Task<GiangVienDto> CreateAsync(GiangVienRequest request)
    {
        if (await _db.Set<GiangVien>().AnyAsync(x => x.Ma == request.Ma))
            throw AppException.Conflict($"Mã giảng viên '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<BoMon>().AnyAsync(x => x.Id == request.BoMonId))
            throw AppException.Conflict($"Bộ môn với Id {request.BoMonId} không tồn tại.");

        var gv = new GiangVien
        {
            Ma = request.Ma.Trim(),
            HoTen = request.HoTen.Trim(),
            HocVi = request.HocVi?.Trim(),
            Email = request.Email?.Trim(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            BoMonId = request.BoMonId
        };

        _db.Add(gv);
        await _db.SaveChangesAsync();

        var boMon = await _db.Set<BoMon>().FindAsync(gv.BoMonId);

        return new GiangVienDto(
            gv.Id, gv.Ma, gv.HoTen, gv.HocVi, gv.Email, gv.SoDienThoai,
            gv.BoMonId, boMon?.Ten, gv.CreatedAt, gv.UpdatedAt
        );
    }

    public async Task<GiangVienDto> UpdateAsync(int id, GiangVienRequest request)
    {
        var gv = await _db.Set<GiangVien>().FindAsync(id);
        if (gv is null) throw AppException.NotFound("giảng viên");

        if (await _db.Set<GiangVien>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw AppException.Conflict($"Mã giảng viên '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<BoMon>().AnyAsync(x => x.Id == request.BoMonId))
            throw AppException.Conflict($"Bộ môn với Id {request.BoMonId} không tồn tại.");

        gv.Ma = request.Ma.Trim();
        gv.HoTen = request.HoTen.Trim();
        gv.HocVi = request.HocVi?.Trim();
        gv.Email = request.Email?.Trim();
        gv.SoDienThoai = request.SoDienThoai?.Trim();
        gv.BoMonId = request.BoMonId;

        await _db.SaveChangesAsync();

        var boMon = await _db.Set<BoMon>().FindAsync(gv.BoMonId);

        return new GiangVienDto(
            gv.Id, gv.Ma, gv.HoTen, gv.HocVi, gv.Email, gv.SoDienThoai,
            gv.BoMonId, boMon?.Ten, gv.CreatedAt, gv.UpdatedAt
        );
    }

    public async Task DeleteAsync(int id)
    {
        var gv = await _db.Set<GiangVien>().FindAsync(id);
        if (gv is null) throw AppException.NotFound("giảng viên");

        _db.Remove(gv);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa giảng viên đang phụ trách lớp hành chính hoặc học phần liên quan.");
        }
    }
}
