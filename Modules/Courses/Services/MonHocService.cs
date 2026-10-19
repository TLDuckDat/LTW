using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Services;

public class MonHocService(AppDbContext db) : IMonHocService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<MonHocDto>> GetPagedAsync(PagedQuery query)
    {
        var q = _db.Set<MonHoc>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s) || x.Ten.Contains(s));
        }

        var total = await q.CountAsync();
        var items = await q.OrderBy(x => x.Ma)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new MonHocDto(x.Id, x.Ma, x.Ten, x.SoTinChi, x.CreatedAt, x.UpdatedAt))
            .ToListAsync();

        return new PagedResult<MonHocDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<MonHocDto> GetByIdAsync(int id)
    {
        var entity = await _db.Set<MonHoc>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? throw AppException.NotFound("môn học") : ToDto(entity);
    }

    public async Task<MonHocDto> CreateAsync(MonHocRequest request)
    {
        var ma = request.Ma.Trim();
        if (await _db.Set<MonHoc>().AnyAsync(x => x.Ma == ma))
            throw AppException.Conflict($"Mã môn học '{ma}' đã tồn tại.");

        var entity = new MonHoc { Ma = ma, Ten = request.Ten.Trim(), SoTinChi = request.SoTinChi };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<MonHocDto> UpdateAsync(int id, MonHocRequest request)
    {
        var entity = await _db.Set<MonHoc>().FindAsync(id);
        if (entity is null) throw AppException.NotFound("môn học");

        var ma = request.Ma.Trim();
        if (await _db.Set<MonHoc>().AnyAsync(x => x.Ma == ma && x.Id != id))
            throw AppException.Conflict($"Mã môn học '{ma}' đã tồn tại.");

        entity.Ma = ma;
        entity.Ten = request.Ten.Trim();
        entity.SoTinChi = request.SoTinChi;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Set<MonHoc>().FindAsync(id);
        if (entity is null) throw AppException.NotFound("môn học");

        if (await _db.Set<MonHocTienQuyet>().AnyAsync(x => x.TienQuyetId == id))
            throw AppException.Conflict("Môn học đang là tiên quyết của môn khác, không thể xóa.");
        if (await _db.Set<LopHocPhan>().AnyAsync(x => x.MonHocId == id))
            throw AppException.Conflict("Môn học đã được mở lớp học phần, không thể xóa.");

        // Quan hệ tiên quyết của chính môn này xóa cùng môn học
        var tienQuyet = await _db.Set<MonHocTienQuyet>().Where(x => x.MonHocId == id).ToListAsync();
        _db.RemoveRange(tienQuyet);
        _db.Remove(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa môn học đang có dữ liệu liên quan.");
        }
    }

    public async Task<IReadOnlyList<MonHocDto>> GetTienQuyetAsync(int id)
    {
        if (!await _db.Set<MonHoc>().AnyAsync(x => x.Id == id)) throw AppException.NotFound("môn học");

        return await (
            from tq in _db.Set<MonHocTienQuyet>().AsNoTracking()
            join mh in _db.Set<MonHoc>().AsNoTracking() on tq.TienQuyetId equals mh.Id
            where tq.MonHocId == id
            orderby mh.Ma
            select new MonHocDto(mh.Id, mh.Ma, mh.Ten, mh.SoTinChi, mh.CreatedAt, mh.UpdatedAt))
            .ToListAsync();
    }

    public async Task<MonHocDto> AddTienQuyetAsync(int id, TienQuyetRequest request)
    {
        var monHoc = await _db.Set<MonHoc>().FindAsync(id);
        var tienQuyet = await _db.Set<MonHoc>().FindAsync(request.TienQuyetId);
        if (monHoc is null || tienQuyet is null) throw AppException.NotFound("môn học hoặc môn tiên quyết");

        if (id == request.TienQuyetId)
            throw AppException.Conflict("Môn học không thể là tiên quyết của chính nó.");
        if (await _db.Set<MonHocTienQuyet>().AnyAsync(x => x.MonHocId == id && x.TienQuyetId == request.TienQuyetId))
            throw AppException.Conflict("Môn tiên quyết này đã được thêm.");
        if (await PhuThuocVongAsync(id, request.TienQuyetId))
            throw AppException.Conflict("Không thể thêm: tạo vòng phụ thuộc giữa các môn tiên quyết.");

        _db.Add(new MonHocTienQuyet { MonHocId = id, TienQuyetId = request.TienQuyetId });
        await _db.SaveChangesAsync();
        return ToDto(tienQuyet);
    }

    public async Task RemoveTienQuyetAsync(int id, int tienQuyetId)
    {
        var entity = await _db.Set<MonHocTienQuyet>()
            .FirstOrDefaultAsync(x => x.MonHocId == id && x.TienQuyetId == tienQuyetId);
        if (entity is null) throw AppException.NotFound("quan hệ tiên quyết");

        _db.Remove(entity);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Kiểm tra việc thêm cạnh "monHocId cần tienQuyetId" có tạo vòng không:
    /// tức là từ tienQuyetId đi theo chuỗi tiên quyết có chạm lại monHocId.
    /// </summary>
    private async Task<bool> PhuThuocVongAsync(int monHocId, int tienQuyetId)
    {
        var cacCanh = await _db.Set<MonHocTienQuyet>().AsNoTracking()
            .Select(x => new { x.MonHocId, x.TienQuyetId })
            .ToListAsync();
        var tienQuyetCua = cacCanh.ToLookup(x => x.MonHocId, x => x.TienQuyetId);

        var daDuyet = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(tienQuyetId);
        while (stack.Count > 0)
        {
            var hienTai = stack.Pop();
            if (hienTai == monHocId) return true;
            if (!daDuyet.Add(hienTai)) continue;
            foreach (var truoc in tienQuyetCua[hienTai]) stack.Push(truoc);
        }
        return false;
    }

    private static MonHocDto ToDto(MonHoc x) => new(x.Id, x.Ma, x.Ten, x.SoTinChi, x.CreatedAt, x.UpdatedAt);
}
