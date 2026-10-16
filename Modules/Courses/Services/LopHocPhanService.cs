using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Courses.Services;

public class LopHocPhanService(AppDbContext db) : ILopHocPhanService
{
    private readonly AppDbContext _db = db;

    // CRS-02 Danh sách lớp học phần, lọc theo học kỳ/môn/giảng viên/trạng thái
    public async Task<PagedResult<LopHocPhanDto>> GetPagedAsync(LopHocPhanQuery query)
    {
        var q = _db.Set<LopHocPhan>().AsNoTracking().AsQueryable();

        if (query.HocKyId is not null) q = q.Where(x => x.HocKyId == query.HocKyId);
        if (query.MonHocId is not null) q = q.Where(x => x.MonHocId == query.MonHocId);
        if (query.GiangVienId is not null) q = q.Where(x => x.GiangVienId == query.GiangVienId);
        if (query.TrangThai is not null) q = q.Where(x => x.TrangThai == query.TrangThai);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s));
        }

        var total = await q.CountAsync();
        var items = await ProjectToDto(q.OrderBy(x => x.Ma)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize))
            .ToListAsync();

        return new PagedResult<LopHocPhanDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<LopHocPhanDto?> GetByIdAsync(int id)
        => await ProjectToDto(_db.Set<LopHocPhan>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync();

    // CRS-02 Mở lớp học phần trong học kỳ
    public async Task<LopHocPhanDto> CreateAsync(LopHocPhanRequest request)
    {
        var ma = request.Ma.Trim();
        if (!await _db.Set<HocKy>().AnyAsync(x => x.Id == request.HocKyId))
            throw new InvalidOperationException("Học kỳ không tồn tại.");
        if (!await _db.Set<MonHoc>().AnyAsync(x => x.Id == request.MonHocId))
            throw new InvalidOperationException("Môn học không tồn tại.");
        if (await _db.Set<LopHocPhan>().AnyAsync(x => x.Ma == ma))
            throw new InvalidOperationException($"Mã lớp học phần '{ma}' đã tồn tại.");

        var entity = new LopHocPhan
        {
            Ma = ma,
            HocKyId = request.HocKyId,
            MonHocId = request.MonHocId,
            SiSoToiDa = request.SiSoToiDa
        };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(entity.Id))!;
    }

    // CRS-02 Cập nhật mã lớp và sĩ số tối đa
    public async Task<LopHocPhanDto?> UpdateAsync(int id, LopHocPhanUpdateRequest request)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return null;

        if (entity.TrangThai is TrangThaiLopHocPhan.DaHuy or TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Lớp học phần đã hủy hoặc kết thúc, không thể chỉnh sửa.");

        var ma = request.Ma.Trim();
        if (await _db.Set<LopHocPhan>().AnyAsync(x => x.Ma == ma && x.Id != id))
            throw new InvalidOperationException($"Mã lớp học phần '{ma}' đã tồn tại.");

        var daDangKy = await DemDangKyAsync(id);
        if (request.SiSoToiDa < daDangKy)
            throw new InvalidOperationException($"Sĩ số tối đa không được nhỏ hơn số sinh viên đã đăng ký ({daDangKy}).");

        // Sĩ số phải vừa với mọi phòng đã xếp lịch cho lớp
        var sucChuaNhoNhat = await _db.Set<LichHoc>()
            .Where(l => l.LopHocPhanId == id && l.PhongHocId != null)
            .Join(_db.Set<PhongHoc>(), l => l.PhongHocId, p => p.Id, (l, p) => (int?)p.SucChua)
            .MinAsync();
        if (sucChuaNhoNhat is not null && request.SiSoToiDa > sucChuaNhoNhat)
            throw new InvalidOperationException($"Sĩ số tối đa vượt sức chứa phòng đã xếp ({sucChuaNhoNhat}).");

        entity.Ma = ma;
        entity.SiSoToiDa = request.SiSoToiDa;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // CRS-02 Xóa lớp học phần chưa có sinh viên đăng ký (lịch học xóa theo lớp)
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return false;

        if (await _db.Set<DangKyHocPhan>().AnyAsync(x => x.LopHocPhanId == id))
            throw new InvalidOperationException("Lớp học phần đã có lượt đăng ký, không thể xóa (hãy hủy lớp).");

        _db.RemoveRange(await _db.Set<LichHoc>().Where(x => x.LopHocPhanId == id).ToListAsync());
        _db.Remove(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa lớp học phần đang có dữ liệu liên quan.");
        }
        return true;
    }

    private Task<int> DemDangKyAsync(int lopHocPhanId)
        => _db.Set<DangKyHocPhan>()
            .CountAsync(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy);

    private IQueryable<LopHocPhanDto> ProjectToDto(IQueryable<LopHocPhan> q)
        => from l in q
           join m in _db.Set<MonHoc>() on l.MonHocId equals m.Id
           select new LopHocPhanDto(
               l.Id, l.Ma, l.HocKyId, l.MonHocId, m.Ma, m.Ten, l.GiangVienId, l.SiSoToiDa,
               _db.Set<DangKyHocPhan>().Count(d => d.LopHocPhanId == l.Id && d.TrangThai == TrangThaiDangKy.DaDangKy),
               l.TrangThai, l.CreatedAt, l.UpdatedAt);
}
