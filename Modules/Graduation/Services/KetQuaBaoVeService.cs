using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Services;

public class KetQuaBaoVeService(AppDbContext db) : IKetQuaBaoVeService
{
    private readonly AppDbContext _db = db;

    public Task<PagedResult<KetQuaBaoVeDto>> GetPagedAsync(KetQuaBaoVeQuery query)
    {
        var data = _db.Set<DeTaiKhoaLuan>().AsNoTracking();
        if (query.SinhVienId is int sinhVienId)
            data = data.Where(x => x.SinhVienId == sinhVienId);
        if (query.HoiDongBaoVeId is int hoiDongId)
            data = data.Where(x => x.HoiDongBaoVeId == hoiDongId);
        if (query.DaCoKetQua is bool daCoKetQua)
            data = data.Where(x => (x.Diem != null) == daCoKetQua);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            data = data.Where(x => x.TenDeTai.Contains(search) ||
                _db.Set<SinhVien>().Any(s => s.Id == x.SinhVienId &&
                    (s.MSSV.Contains(search) || s.HoTen.Contains(search))));
        }

        return data.OrderByDescending(x => x.Id).ToPagedResultAsync(query, Project);
    }

    public async Task<KetQuaBaoVeDto> GetByDeTaiIdAsync(int deTaiId) =>
        await Project(_db.Set<DeTaiKhoaLuan>().AsNoTracking().Where(x => x.Id == deTaiId)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("đề tài");

    public async Task<KetQuaBaoVeDto> SaveAsync(int deTaiId, NhapKetQuaBaoVeRequest request)
    {
        var diem = request.Diem ?? throw AppException.BadRequest("Vui lòng nhập điểm bảo vệ.");
        if (decimal.Round(diem, 2) != diem)
            throw AppException.BadRequest("Điểm bảo vệ có tối đa 2 chữ số thập phân.");
        var nhanXet = request.NhanXetBaoVe?.Trim();

        var deTai = await _db.Set<DeTaiKhoaLuan>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == deTaiId)
            ?? throw AppException.NotFound("đề tài");
        if (deTai.SinhVienId is null)
            throw AppException.Conflict("Đề tài chưa có sinh viên đăng ký.");
        if (deTai.HoiDongBaoVeId is null)
            throw AppException.Conflict("Đề tài chưa được phân công vào hội đồng bảo vệ.");

        var now = DateTime.UtcNow;
        var updated = await _db.Set<DeTaiKhoaLuan>()
            .Where(x => x.Id == deTaiId && x.SinhVienId != null &&
                x.HoiDongBaoVeId == deTai.HoiDongBaoVeId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Diem, diem)
                .SetProperty(x => x.NhanXetBaoVe, nhanXet)
                .SetProperty(x => x.NgayGhiNhanKetQua, now)
                .SetProperty(x => x.UpdatedAt, now));
        if (updated == 0)
            throw AppException.Conflict("Thông tin đề tài đã thay đổi. Hãy tải lại trước khi nhập kết quả.");
        return await GetByDeTaiIdAsync(deTaiId);
    }

    private IQueryable<KetQuaBaoVeDto> Project(IQueryable<DeTaiKhoaLuan> data) =>
        from dt in data
        join sv0 in _db.Set<SinhVien>().AsNoTracking() on dt.SinhVienId equals sv0.Id into sinhVienGroup
        from sv in sinhVienGroup.DefaultIfEmpty()
        join hd0 in _db.Set<HoiDongBaoVe>().AsNoTracking() on dt.HoiDongBaoVeId equals hd0.Id into hoiDongGroup
        from hd in hoiDongGroup.DefaultIfEmpty()
        select new KetQuaBaoVeDto(dt.Id, dt.TenDeTai,
            dt.SinhVienId, sv == null ? null : sv.MSSV, sv == null ? null : sv.HoTen,
            dt.HoiDongBaoVeId, hd == null ? null : hd.Ten,
            dt.Diem, dt.NhanXetBaoVe, dt.NgayGhiNhanKetQua, dt.Diem != null);
}
