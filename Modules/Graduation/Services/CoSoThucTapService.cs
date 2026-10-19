using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Entities;

namespace StudentManagement.Modules.Graduation.Services;

public class CoSoThucTapService(AppDbContext db) : ICoSoThucTapService
{
    private const string MaDaTonTai = "Mã cơ sở đã tồn tại.";

    private readonly AppDbContext _db = db;

    public Task<PagedResult<CoSoThucTapDto>> GetPagedAsync(PagedQuery query)
    {
        var data = _db.Set<CoSoThucTap>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            data = data.Where(x => x.Ma.Contains(search) || x.Ten.Contains(search));
        }

        return data.OrderBy(x => x.Ma).ToPagedResultAsync(query, page => page.Select(x => ToDto(x)));
    }

    public async Task<CoSoThucTapDto> GetByIdAsync(int id)
    {
        var item = await _db.Set<CoSoThucTap>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("cơ sở thực tập");
        return ToDto(item);
    }

    public async Task<CoSoThucTapDto> CreateAsync(CoSoThucTapRequest request)
    {
        var ma = request.Ma.Trim();
        if (await _db.Set<CoSoThucTap>().AnyAsync(x => x.Ma == ma))
            throw AppException.Conflict(MaDaTonTai);

        var item = new CoSoThucTap();
        Fill(item, request);
        _db.Set<CoSoThucTap>().Add(item);
        await SaveAsync();
        return ToDto(item);
    }

    public async Task<CoSoThucTapDto> UpdateAsync(int id, CoSoThucTapRequest request)
    {
        var item = await FindAsync(id);
        var ma = request.Ma.Trim();
        if (await _db.Set<CoSoThucTap>().AnyAsync(x => x.Ma == ma && x.Id != id))
            throw AppException.Conflict(MaDaTonTai);

        Fill(item, request);
        await SaveAsync();
        return ToDto(item);
    }

    public async Task<CoSoThucTapDto> SetTrangThaiAsync(int id, TrangThaiCoSoRequest request)
    {
        var item = await FindAsync(id);
        item.DangTiepNhan = request.DangTiepNhan;
        await _db.SaveChangesAsync();
        return ToDto(item);
    }

    public async Task DeleteAsync(int id)
    {
        var item = await FindAsync(id);
        if (await _db.Set<ThucTap>().AnyAsync(x => x.CoSoThucTapId == id))
            throw AppException.Conflict("Cơ sở đã có đăng ký thực tập; hãy ngừng tiếp nhận thay vì xóa.");

        _db.Set<CoSoThucTap>().Remove(item);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            throw AppException.Conflict("Cơ sở đã có dữ liệu liên quan; hãy ngừng tiếp nhận thay vì xóa.");
        }
    }

    private async Task<CoSoThucTap> FindAsync(int id) =>
        await _db.Set<CoSoThucTap>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("cơ sở thực tập");

    private async Task SaveAsync()
    {
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw AppException.Conflict(MaDaTonTai);
        }
    }

    private static void Fill(CoSoThucTap item, CoSoThucTapRequest request)
    {
        item.Ma = request.Ma.Trim();
        item.Ten = request.Ten.Trim();
        item.DiaChi = request.DiaChi?.Trim();
        item.SoDienThoai = request.SoDienThoai?.Trim();
        item.Email = request.Email?.Trim();
        item.NguoiLienHe = request.NguoiLienHe?.Trim();
    }

    private static CoSoThucTapDto ToDto(CoSoThucTap item) =>
        new(item.Id, item.Ma, item.Ten, item.DiaChi, item.SoDienThoai,
            item.Email, item.NguoiLienHe, item.DangTiepNhan);
}
