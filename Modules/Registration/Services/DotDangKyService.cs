using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Registration.Dtos;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Registration.Services;

public class DotDangKyService(AppDbContext db) : IDotDangKyService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<DotDangKyDto>> GetPagedAsync(DotDangKyQuery query)
    {
        var q = _db.Set<DotDangKy>().AsNoTracking().AsQueryable();

        if (query.HocKyId is not null) q = q.Where(x => x.HocKyId == query.HocKyId);
        if (query.TrangThai is not null) q = q.Where(x => x.TrangThai == query.TrangThai);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ten.Contains(s));
        }

        var total = await q.CountAsync();
        var items = (await q.OrderByDescending(x => x.NgayBatDau)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync())
            .Select(ToDto)
            .ToList();

        return new PagedResult<DotDangKyDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<DotDangKyDto> GetByIdAsync(int id)
    {
        var entity = await _db.Set<DotDangKy>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? throw AppException.NotFound("đợt đăng ký") : ToDto(entity);
    }

    // REG-01 Tạo đợt đăng ký cho học kỳ, ban đầu chưa mở
    public async Task<DotDangKyDto> CreateAsync(DotDangKyRequest request)
    {
        KiemTraThoiGian(request.NgayBatDau, request.NgayKetThuc);
        if (!await _db.Set<HocKy>().AnyAsync(x => x.Id == request.HocKyId))
            throw AppException.Conflict("Học kỳ không tồn tại.");

        var entity = new DotDangKy
        {
            Ten = request.Ten.Trim(),
            HocKyId = request.HocKyId,
            NgayBatDau = request.NgayBatDau,
            NgayKetThuc = request.NgayKetThuc
        };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<DotDangKyDto> UpdateAsync(int id, DotDangKyUpdateRequest request)
    {
        var entity = await _db.Set<DotDangKy>().FindAsync(id);
        if (entity is null) throw AppException.NotFound("đợt đăng ký");

        if (entity.TrangThai == TrangThaiDotDangKy.DaDong)
            throw AppException.Conflict("Đợt đăng ký đã đóng, không thể chỉnh sửa.");
        KiemTraThoiGian(request.NgayBatDau, request.NgayKetThuc);

        entity.Ten = request.Ten.Trim();
        entity.NgayBatDau = request.NgayBatDau;
        entity.NgayKetThuc = request.NgayKetThuc;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    // Chỉ xóa đợt chưa mở; đợt đã mở/đóng giữ lại làm lịch sử
    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Set<DotDangKy>().FindAsync(id);
        if (entity is null) throw AppException.NotFound("đợt đăng ký");

        if (entity.TrangThai != TrangThaiDotDangKy.ChuaMo)
            throw AppException.Conflict("Chỉ xóa được đợt đăng ký chưa mở.");

        _db.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // Mỗi học kỳ chỉ có một đợt đang mở tại một thời điểm
    public async Task<DotDangKyDto> MoAsync(int id)
    {
        var entity = await _db.Set<DotDangKy>().FindAsync(id);
        if (entity is null) throw AppException.NotFound("đợt đăng ký");

        if (entity.TrangThai != TrangThaiDotDangKy.ChuaMo)
            throw AppException.Conflict("Chỉ mở được đợt đăng ký đang ở trạng thái chưa mở.");
        if (await _db.Set<DotDangKy>().AnyAsync(x =>
                x.HocKyId == entity.HocKyId && x.TrangThai == TrangThaiDotDangKy.DangMo))
            throw AppException.Conflict("Học kỳ này đã có một đợt đăng ký đang mở.");

        entity.TrangThai = TrangThaiDotDangKy.DangMo;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<DotDangKyDto> DongAsync(int id)
    {
        var entity = await _db.Set<DotDangKy>().FindAsync(id);
        if (entity is null) throw AppException.NotFound("đợt đăng ký");

        if (entity.TrangThai != TrangThaiDotDangKy.DangMo)
            throw AppException.Conflict("Chỉ đóng được đợt đăng ký đang mở.");

        entity.TrangThai = TrangThaiDotDangKy.DaDong;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    private static void KiemTraThoiGian(DateTime batDau, DateTime ketThuc)
    {
        if (ketThuc <= batDau)
            throw AppException.Conflict("Thời điểm kết thúc phải sau thời điểm bắt đầu.");
    }

    private static DotDangKyDto ToDto(DotDangKy x) =>
        new(x.Id, x.Ten, x.HocKyId, x.NgayBatDau, x.NgayKetThuc, x.TrangThai.ToString(), x.CreatedAt, x.UpdatedAt);
}
