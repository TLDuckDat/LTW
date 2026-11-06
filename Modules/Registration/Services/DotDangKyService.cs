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
        await CapNhatTrangThaiTheoThoiGianAsync();

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
        await CapNhatTrangThaiTheoThoiGianAsync();

        var entity = await _db.Set<DotDangKy>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? throw AppException.NotFound("đợt đăng ký") : ToDto(entity);
    }

    // REG-01 Tạo đợt đăng ký cho học kỳ, ban đầu chưa mở
    public async Task<DotDangKyDto> CreateAsync(DotDangKyRequest request)
    {
        var (batDau, ketThuc) = KiemTraThoiGian(request.NgayBatDau, request.NgayKetThuc);
        if (batDau < DateTime.UtcNow.AddMinutes(-5))
            throw AppException.BadRequest("Thời điểm bắt đầu không được sớm hơn hiện tại.");
        if (!await _db.Set<HocKy>().AnyAsync(x => x.Id == request.HocKyId))
            throw AppException.Conflict("Học kỳ không tồn tại.");

        var entity = new DotDangKy
        {
            Ten = request.Ten.Trim(),
            HocKyId = request.HocKyId,
            NgayBatDau = batDau,
            NgayKetThuc = ketThuc
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
        var (batDau, ketThuc) = KiemTraThoiGian(request.NgayBatDau, request.NgayKetThuc);
        // Chỉ kiểm tra khi đổi mốc bắt đầu, để sửa tên hoặc mốc kết thúc không bị chặn bởi mốc cũ đã qua
        if (entity.TrangThai == TrangThaiDotDangKy.ChuaMo && batDau != entity.NgayBatDau && batDau < DateTime.UtcNow.AddMinutes(-5))
            throw AppException.BadRequest("Thời điểm bắt đầu không được sớm hơn hiện tại.");

        entity.Ten = request.Ten.Trim();
        entity.NgayBatDau = batDau;
        entity.NgayKetThuc = ketThuc;
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
        await CapNhatTrangThaiTheoThoiGianAsync();

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
        await CapNhatTrangThaiTheoThoiGianAsync();

        var entity = await _db.Set<DotDangKy>().FindAsync(id);
        if (entity is null) throw AppException.NotFound("đợt đăng ký");

        if (entity.TrangThai != TrangThaiDotDangKy.DangMo)
            throw AppException.Conflict("Chỉ đóng được đợt đăng ký đang mở.");

        entity.TrangThai = TrangThaiDotDangKy.DaDong;
        await _db.SaveChangesAsync();
        return ToDto(entity);
    }

    // Chuẩn hóa về UTC trước khi so sánh: hai mốc có thể gửi lên với múi giờ khác nhau
    private static (DateTime BatDau, DateTime KetThuc) KiemTraThoiGian(DateTime batDau, DateTime ketThuc)
    {
        batDau = ThoiGian.VeUtc(batDau);
        ketThuc = ThoiGian.VeUtc(ketThuc);
        if (ketThuc <= batDau)
            throw AppException.Conflict("Thời điểm kết thúc phải sau thời điểm bắt đầu.");
        if (ketThuc <= DateTime.UtcNow)
            throw AppException.BadRequest("Thời điểm kết thúc phải sau thời điểm hiện tại.");
        return (batDau, ketThuc);
    }

    // Trạng thái theo thời gian: đợt đang mở đã hết hạn thì đóng; đợt chưa mở đến giờ thì mở (mỗi học kỳ một đợt đang mở).
    // Đợt đã đóng bằng tay không tự mở lại, vì chỉ đợt ChuaMo mới được xét mở.
    private async Task CapNhatTrangThaiTheoThoiGianAsync()
    {
        var now = DateTime.UtcNow;

        var hetHan = await _db.Set<DotDangKy>()
            .Where(x => x.TrangThai != TrangThaiDotDangKy.DaDong && x.NgayKetThuc <= now)
            .ToListAsync();
        foreach (var dot in hetHan) dot.TrangThai = TrangThaiDotDangKy.DaDong;

        var hocKyDangMo = await _db.Set<DotDangKy>()
            .Where(x => x.TrangThai == TrangThaiDotDangKy.DangMo && x.NgayKetThuc > now)
            .Select(x => x.HocKyId)
            .ToListAsync();

        var choMo = await _db.Set<DotDangKy>()
            .Where(x => x.TrangThai == TrangThaiDotDangKy.ChuaMo && x.NgayBatDau <= now && now < x.NgayKetThuc)
            .OrderBy(x => x.NgayBatDau)
            .ToListAsync();
        foreach (var dot in choMo)
        {
            if (hocKyDangMo.Contains(dot.HocKyId)) continue;
            dot.TrangThai = TrangThaiDotDangKy.DangMo;
            hocKyDangMo.Add(dot.HocKyId);
        }

        if (_db.ChangeTracker.HasChanges()) await _db.SaveChangesAsync();
    }

    private static DotDangKyDto ToDto(DotDangKy x) =>
        new(x.Id, x.Ten, x.HocKyId, x.NgayBatDau, x.NgayKetThuc, x.TrangThai.ToString(), x.CreatedAt, x.UpdatedAt);
}
