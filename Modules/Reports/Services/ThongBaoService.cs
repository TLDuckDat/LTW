using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Reports.Dtos;
using StudentManagement.Modules.Reports.Entities;

namespace StudentManagement.Modules.Reports.Services;

public class ThongBaoService(AppDbContext db) : IThongBaoService
{
    private readonly AppDbContext _db = db;

    public Task<PagedResult<ThongBaoDto>> GetPagedAsync(PagedQuery query, bool chiDaDang)
    {
        var thongBao = Visible(chiDaDang);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            thongBao = thongBao.Where(x => x.TieuDe.Contains(search) || x.NoiDung.Contains(search));
        }

        return thongBao.OrderByDescending(x => x.NgayDang).ThenByDescending(x => x.Id)
            .ToPagedResultAsync(query, Project);
    }

    // Thông báo hẹn giờ chưa tới giờ đăng coi như không tồn tại với người dùng thường
    public async Task<ThongBaoDto> GetByIdAsync(int id, bool chiDaDang) =>
        await Project(Visible(chiDaDang).Where(x => x.Id == id)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("thông báo");

    public async Task<ThongBaoDto> CreateAsync(ThongBaoRequest request, int nguoiTaoId)
    {
        var thongBao = new ThongBao { NguoiTaoId = nguoiTaoId };
        Apply(thongBao, request);
        _db.Add(thongBao);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(thongBao.Id, chiDaDang: false);
    }

    public async Task<ThongBaoDto> UpdateAsync(int id, ThongBaoRequest request)
    {
        var thongBao = await _db.Set<ThongBao>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("thông báo");
        Apply(thongBao, request);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id, chiDaDang: false);
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await _db.Set<ThongBao>().Where(x => x.Id == id).ExecuteDeleteAsync();
        if (deleted == 0) throw AppException.NotFound("thông báo");
    }

    private static void Apply(ThongBao thongBao, ThongBaoRequest request)
    {
        var tieuDe = request.TieuDe.Trim();
        var noiDung = request.NoiDung.Trim();
        if (tieuDe.Length == 0 || noiDung.Length == 0)
            throw AppException.BadRequest("Tiêu đề và nội dung thông báo không được để trống.");

        thongBao.TieuDe = tieuDe;
        thongBao.NoiDung = noiDung;
        thongBao.NgayDang = request.NgayDang is { } ngayDang
            ? ThoiGian.VeUtc(ngayDang)
            : DateTime.UtcNow;
    }

    private IQueryable<ThongBao> Visible(bool chiDaDang)
    {
        var thongBao = _db.Set<ThongBao>().AsNoTracking();
        if (!chiDaDang) return thongBao;
        var now = DateTime.UtcNow;
        return thongBao.Where(x => x.NgayDang <= now);
    }

    private IQueryable<ThongBaoDto> Project(IQueryable<ThongBao> thongBao) =>
        from tb in thongBao
        join tk in _db.Set<TaiKhoan>().AsNoTracking() on tb.NguoiTaoId equals tk.Id into nhom
        from tk in nhom.DefaultIfEmpty()
        select new ThongBaoDto(tb.Id, tb.TieuDe, tb.NoiDung, tb.NgayDang, tb.NguoiTaoId,
            tk == null ? null : tk.HoTen, tb.CreatedAt, tb.UpdatedAt);
}
