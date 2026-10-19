using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Services;

public class HoiDongService(AppDbContext db) : IHoiDongService
{
    private readonly AppDbContext _db = db;

    public Task<PagedResult<HoiDongDto>> GetPagedAsync(PagedQuery query)
    {
        var data = _db.Set<HoiDongBaoVe>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            data = data.Where(x => x.Ten.Contains(search));
        }

        return data.OrderByDescending(x => x.NgayBaoVe).ThenByDescending(x => x.Id)
            .ToPagedResultAsync(query, ProjectHoiDong);
    }

    public async Task<HoiDongDto> GetByIdAsync(int id) =>
        await ProjectHoiDong(_db.Set<HoiDongBaoVe>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("hội đồng bảo vệ");

    public async Task<HoiDongDto> CreateAsync(HoiDongRequest request)
    {
        await ValidateAsync(request);
        var item = new HoiDongBaoVe();
        Fill(item, request);
        _db.Set<HoiDongBaoVe>().Add(item);
        await SaveAsync();
        return await GetByIdAsync(item.Id);
    }

    public async Task<HoiDongDto> UpdateAsync(int id, HoiDongRequest request)
    {
        var item = await _db.Set<HoiDongBaoVe>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("hội đồng bảo vệ");
        await ValidateAsync(request);
        Fill(item, request);
        await SaveAsync();
        return await GetByIdAsync(id);
    }

    public async Task<PagedResult<DeTaiHoiDongDto>> GetDeTaiAsync(int id, PagedQuery query)
    {
        if (!await _db.Set<HoiDongBaoVe>().AsNoTracking().AnyAsync(x => x.Id == id))
            throw AppException.NotFound("hội đồng bảo vệ");

        var data = _db.Set<DeTaiKhoaLuan>().AsNoTracking()
            .Where(x => x.HoiDongBaoVeId == id);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            data = data.Where(x => x.TenDeTai.Contains(search) ||
                _db.Set<SinhVien>().Any(s => s.Id == x.SinhVienId && s.MSSV.Contains(search)));
        }

        return await data.OrderBy(x => x.Id).ToPagedResultAsync(query, ProjectDeTai);
    }

    public async Task<PhanCongDeTaiDto> PhanCongDeTaiAsync(int id, int deTaiId)
    {
        if (!await _db.Set<HoiDongBaoVe>().AsNoTracking().AnyAsync(x => x.Id == id))
            throw AppException.NotFound("hội đồng bảo vệ");
        var deTai = await _db.Set<DeTaiKhoaLuan>().FirstOrDefaultAsync(x => x.Id == deTaiId)
            ?? throw AppException.NotFound("đề tài");
        if (deTai.SinhVienId is null)
            throw AppException.Conflict("Đề tài chưa có sinh viên đăng ký.");

        if (deTai.HoiDongBaoVeId != id)
        {
            if (deTai.Diem is not null)
                throw AppException.Conflict("Đề tài đã có điểm bảo vệ nên không thể chuyển hội đồng.");
            try
            {
                var updated = await _db.Set<DeTaiKhoaLuan>()
                    .Where(x => x.Id == deTaiId && x.Diem == null)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.HoiDongBaoVeId, id)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
                if (updated == 0)
                    throw AppException.Conflict("Đề tài đã có điểm bảo vệ nên không thể chuyển hội đồng.");
            }
            catch (SqlException ex) when (ex.IsForeignKeyViolation())
            {
                throw AppException.Conflict("Hội đồng không còn tồn tại. Hãy kiểm tra lại.");
            }
        }
        return new PhanCongDeTaiDto(deTai.Id, id);
    }

    private IQueryable<HoiDongDto> ProjectHoiDong(IQueryable<HoiDongBaoVe> data) =>
        from hd in data
        join chuTich in _db.Set<GiangVien>().AsNoTracking() on hd.ChuTichId equals chuTich.Id
        join thuKy0 in _db.Set<GiangVien>().AsNoTracking() on hd.ThuKyId equals thuKy0.Id into thuKyGroup
        from thuKy in thuKyGroup.DefaultIfEmpty()
        select new HoiDongDto(hd.Id, hd.Ten, hd.NgayBaoVe,
            hd.ChuTichId, chuTich.HoTen, hd.ThuKyId,
            thuKy == null ? null : thuKy.HoTen,
            _db.Set<DeTaiKhoaLuan>().Count(x => x.HoiDongBaoVeId == hd.Id));

    private IQueryable<DeTaiHoiDongDto> ProjectDeTai(IQueryable<DeTaiKhoaLuan> data) =>
        from dt in data
        join sv0 in _db.Set<SinhVien>().AsNoTracking() on dt.SinhVienId equals sv0.Id into sinhVienGroup
        from sv in sinhVienGroup.DefaultIfEmpty()
        join gv0 in _db.Set<GiangVien>().AsNoTracking() on dt.GiangVienHuongDanId equals gv0.Id into giangVienGroup
        from gv in giangVienGroup.DefaultIfEmpty()
        select new DeTaiHoiDongDto(dt.Id, dt.TenDeTai, dt.SinhVienId,
            sv == null ? null : sv.MSSV, sv == null ? null : sv.HoTen,
            dt.GiangVienHuongDanId, gv == null ? null : gv.HoTen);

    private async Task ValidateAsync(HoiDongRequest request)
    {
        if (request.NgayBaoVe == default)
            throw AppException.BadRequest("Ngày bảo vệ không hợp lệ.");
        if (request.ThuKyId == request.ChuTichId)
            throw AppException.BadRequest("Chủ tịch và thư ký phải là hai giảng viên khác nhau.");
        if (!await _db.Set<GiangVien>().AsNoTracking().AnyAsync(x => x.Id == request.ChuTichId))
            throw AppException.NotFound("giảng viên làm chủ tịch");
        if (request.ThuKyId is int thuKyId &&
            !await _db.Set<GiangVien>().AsNoTracking().AnyAsync(x => x.Id == thuKyId))
            throw AppException.NotFound("giảng viên làm thư ký");
    }

    private async Task SaveAsync()
    {
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            throw AppException.Conflict("Giảng viên không còn tồn tại. Hãy kiểm tra lại.");
        }
    }

    private static void Fill(HoiDongBaoVe item, HoiDongRequest request)
    {
        item.Ten = request.Ten.Trim();
        item.NgayBaoVe = request.NgayBaoVe;
        item.ChuTichId = request.ChuTichId;
        item.ThuKyId = request.ThuKyId;
    }
}
