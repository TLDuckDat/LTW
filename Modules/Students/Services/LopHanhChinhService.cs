using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Services;

public class LopHanhChinhService(AppDbContext db) : ILopHanhChinhService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<LopHanhChinhDto>> GetPagedAsync(PagedQuery query, int? nganhId = null, int? khoaTuyen = null)
    {
        var q = _db.Set<LopHanhChinh>().AsNoTracking();

        if (nganhId.HasValue)
        {
            q = q.Where(x => x.NganhId == nganhId.Value);
        }

        if (khoaTuyen.HasValue)
        {
            q = q.Where(x => x.KhoaTuyen == khoaTuyen.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.Ten.Contains(s));
        }

        var total = await q.CountAsync();

        var queryJoin = from lop in q
                        join ng in _db.Set<Nganh>().AsNoTracking() on lop.NganhId equals ng.Id into ngGroup
                        from n in ngGroup.DefaultIfEmpty()
                        join gv in _db.Set<GiangVien>().AsNoTracking() on lop.CoVanId equals gv.Id into gvGroup
                        from g in gvGroup.DefaultIfEmpty()
                        orderby lop.Ma
                        select new LopHanhChinhDto(
                            lop.Id,
                            lop.Ma,
                            lop.Ten,
                            lop.NganhId,
                            n != null ? n.Ten : null,
                            lop.KhoaTuyen,
                            lop.CoVanId,
                            g != null ? g.HoTen : null,
                            _db.Set<SinhVien>().Count(sv => sv.LopHanhChinhId == lop.Id),
                            lop.CreatedAt,
                            lop.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<LopHanhChinhDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<LopHanhChinhDetailDto> GetByIdAsync(int id)
    {
        var lop = await _db.Set<LopHanhChinh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (lop is null) throw AppException.NotFound("lớp hành chính");

        var nganh = await _db.Set<Nganh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lop.NganhId);
        GiangVien? coVan = null;
        if (lop.CoVanId.HasValue)
        {
            coVan = await _db.Set<GiangVien>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lop.CoVanId.Value);
        }

        var sinhViens = await (from sv in _db.Set<SinhVien>().AsNoTracking()
                               where sv.LopHanhChinhId == lop.Id
                               orderby sv.MSSV
                               select new SinhVienDto(
                                   sv.Id,
                                   sv.MSSV,
                                   sv.HoTen,
                                   sv.NgaySinh,
                                   sv.GioiTinh,
                                   sv.Email,
                                   sv.SoDienThoai,
                                   sv.NganhId,
                                   nganh != null ? nganh.Ten : null,
                                   sv.LopHanhChinhId,
                                   lop.Ten,
                                   sv.TrangThai,
                                   sv.CreatedAt,
                                   sv.UpdatedAt
                               )).ToListAsync();

        return new LopHanhChinhDetailDto(
            lop.Id,
            lop.Ma,
            lop.Ten,
            lop.NganhId,
            nganh?.Ten,
            lop.KhoaTuyen,
            lop.CoVanId,
            coVan?.HoTen,
            sinhViens,
            lop.CreatedAt,
            lop.UpdatedAt
        );
    }

    public async Task<LopHanhChinhDto> CreateAsync(LopHanhChinhRequest request)
    {
        if (await _db.Set<LopHanhChinh>().AnyAsync(x => x.Ma == request.Ma))
            throw AppException.Conflict($"Mã lớp '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw AppException.Conflict($"Ngành với Id {request.NganhId} không tồn tại.");

        if (request.CoVanId.HasValue && !await _db.Set<GiangVien>().AnyAsync(x => x.Id == request.CoVanId.Value))
            throw AppException.Conflict($"Giảng viên cố vấn với Id {request.CoVanId.Value} không tồn tại.");

        var lop = new LopHanhChinh
        {
            Ma = request.Ma.Trim(),
            Ten = request.Ten.Trim(),
            NganhId = request.NganhId,
            KhoaTuyen = request.KhoaTuyen,
            CoVanId = request.CoVanId
        };

        _db.Add(lop);
        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(lop.NganhId);
        var coVan = lop.CoVanId.HasValue ? await _db.Set<GiangVien>().FindAsync(lop.CoVanId.Value) : null;

        return new LopHanhChinhDto(
            lop.Id, lop.Ma, lop.Ten, lop.NganhId, nganh?.Ten, lop.KhoaTuyen,
            lop.CoVanId, coVan?.HoTen, 0, lop.CreatedAt, lop.UpdatedAt
        );
    }

    public async Task<LopHanhChinhDto> UpdateAsync(int id, LopHanhChinhRequest request)
    {
        var lop = await _db.Set<LopHanhChinh>().FindAsync(id);
        if (lop is null) throw AppException.NotFound("lớp hành chính");

        if (await _db.Set<LopHanhChinh>().AnyAsync(x => x.Ma == request.Ma && x.Id != id))
            throw AppException.Conflict($"Mã lớp '{request.Ma}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw AppException.Conflict($"Ngành với Id {request.NganhId} không tồn tại.");

        if (request.CoVanId.HasValue && !await _db.Set<GiangVien>().AnyAsync(x => x.Id == request.CoVanId.Value))
            throw AppException.Conflict($"Giảng viên cố vấn với Id {request.CoVanId.Value} không tồn tại.");

        lop.Ma = request.Ma.Trim();
        lop.Ten = request.Ten.Trim();
        lop.NganhId = request.NganhId;
        lop.KhoaTuyen = request.KhoaTuyen;
        lop.CoVanId = request.CoVanId;

        await _db.SaveChangesAsync();

        var nganh = await _db.Set<Nganh>().FindAsync(lop.NganhId);
        var coVan = lop.CoVanId.HasValue ? await _db.Set<GiangVien>().FindAsync(lop.CoVanId.Value) : null;
        var soSv = await _db.Set<SinhVien>().CountAsync(sv => sv.LopHanhChinhId == lop.Id);

        return new LopHanhChinhDto(
            lop.Id, lop.Ma, lop.Ten, lop.NganhId, nganh?.Ten, lop.KhoaTuyen,
            lop.CoVanId, coVan?.HoTen, soSv, lop.CreatedAt, lop.UpdatedAt
        );
    }

    public async Task DeleteAsync(int id)
    {
        var lop = await _db.Set<LopHanhChinh>().FindAsync(id);
        if (lop is null) throw AppException.NotFound("lớp hành chính");

        _db.Remove(lop);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa lớp hành chính đang có sinh viên tham gia.");
        }
    }

    // STU-04: Phân danh sách sinh viên vào lớp
    public async Task PhanLopAsync(int id, PhanLopRequest request)
    {
        var lop = await _db.Set<LopHanhChinh>().FindAsync(id);
        if (lop is null) throw AppException.NotFound("lớp hành chính");

        var sinhViens = await _db.Set<SinhVien>()
            .Where(x => request.SinhVienIds.Contains(x.Id))
            .ToListAsync();

        foreach (var sv in sinhViens)
        {
            sv.LopHanhChinhId = id;
        }

        await _db.SaveChangesAsync();
    }

    // STU-05: Gán cố vấn học tập cho lớp
    public async Task PhanCoVanAsync(int id, PhanCoVanRequest request)
    {
        var lop = await _db.Set<LopHanhChinh>().FindAsync(id);
        if (lop is null) throw AppException.NotFound("lớp hành chính");

        if (request.CoVanId.HasValue && !await _db.Set<GiangVien>().AnyAsync(x => x.Id == request.CoVanId.Value))
            throw AppException.BadRequest($"Giảng viên với Id {request.CoVanId.Value} không tồn tại.");

        lop.CoVanId = request.CoVanId;
        await _db.SaveChangesAsync();
    }
}
