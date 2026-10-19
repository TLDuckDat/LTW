using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Services;

public class MienGiamService(AppDbContext db) : IMienGiamService
{
    private const string DaTonTai = "Sinh viên đã có hồ sơ miễn giảm trong học kỳ này.";

    private readonly AppDbContext _db = db;

    public Task<PagedResult<MienGiamDto>> GetPagedAsync(SinhVienHocKyQuery query)
    {
        var data = _db.Set<MienGiam>().AsNoTracking();
        if (query.SinhVienId is int sinhVienId) data = data.Where(x => x.SinhVienId == sinhVienId);
        if (query.HocKyId is int hocKyId) data = data.Where(x => x.HocKyId == hocKyId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var sinhVien = _db.SinhVienPhuHop(query.Search);
            data = data.Where(x => sinhVien.Contains(x.SinhVienId));
        }

        return data.OrderByDescending(x => x.HocKyId).ThenBy(x => x.SinhVienId).ToPagedResultAsync(query, Project);
    }

    public async Task<MienGiamDto> GetByIdAsync(int id) =>
        await Project(_db.Set<MienGiam>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("hồ sơ miễn giảm");

    public async Task<MienGiamDto> CreateAsync(MienGiamRequest request)
    {
        await _db.GetSinhVienAsync(request.SinhVienId);
        await _db.GetHocKyAsync(request.HocKyId);
        KiemTraTyLe(request.TyLe);
        if (await _db.Set<MienGiam>().AnyAsync(x => x.SinhVienId == request.SinhVienId && x.HocKyId == request.HocKyId))
            throw AppException.Conflict(DaTonTai);

        var mienGiam = new MienGiam
        {
            SinhVienId = request.SinhVienId,
            HocKyId = request.HocKyId,
            TyLe = request.TyLe,
            LyDo = request.LyDo?.Trim()
        };
        _db.Add(mienGiam);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw AppException.Conflict(DaTonTai);
        }
        return await GetByIdAsync(mienGiam.Id);
    }

    public async Task<MienGiamDto> UpdateAsync(int id, CapNhatMienGiamRequest request)
    {
        var mienGiam = await _db.Set<MienGiam>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("hồ sơ miễn giảm");
        KiemTraTyLe(request.TyLe);
        mienGiam.TyLe = request.TyLe;
        mienGiam.LyDo = request.LyDo?.Trim();
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        if (await _db.Set<MienGiam>().Where(x => x.Id == id).ExecuteDeleteAsync() == 0)
            throw AppException.NotFound("hồ sơ miễn giảm");
    }

    // Cột decimal(5,2): 12.5 hợp lệ, 12.345 thì không
    private static void KiemTraTyLe(decimal tyLe)
    {
        if (decimal.Round(tyLe, 2) != tyLe)
            throw AppException.BadRequest("Tỷ lệ miễn giảm chỉ được có tối đa 2 chữ số thập phân.");
    }

    private IQueryable<MienGiamDto> Project(IQueryable<MienGiam> data) =>
        from mg in data
        join sv in _db.Set<SinhVien>().AsNoTracking() on mg.SinhVienId equals sv.Id
        join hk in _db.Set<HocKy>().AsNoTracking() on mg.HocKyId equals hk.Id
        select new MienGiamDto(mg.Id, sv.Id, sv.MSSV, sv.HoTen, hk.Id, hk.Ten,
            mg.TyLe, mg.LyDo, mg.CreatedAt, mg.UpdatedAt);
}
