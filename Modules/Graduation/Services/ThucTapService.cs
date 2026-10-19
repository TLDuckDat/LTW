using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Services;

public class ThucTapService(AppDbContext db) : IThucTapService
{
    private readonly AppDbContext _db = db;

    public Task<PagedResult<ThucTapDto>> GetPagedAsync(ThucTapQuery query)
    {
        var data = _db.Set<ThucTap>().AsNoTracking();
        if (query.SinhVienId is int sinhVienId) data = data.Where(x => x.SinhVienId == sinhVienId);
        if (query.HocKyId is int hocKyId) data = data.Where(x => x.HocKyId == hocKyId);
        if (query.CoSoThucTapId is int coSoId) data = data.Where(x => x.CoSoThucTapId == coSoId);
        if (query.TrangThai is TrangThaiThucTap trangThai)
            data = data.Where(x => x.TrangThai == trangThai);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            data = data.Where(x =>
                _db.Set<SinhVien>().Any(s => s.Id == x.SinhVienId &&
                    (s.MSSV.Contains(search) || s.HoTen.Contains(search))) ||
                _db.Set<CoSoThucTap>().Any(c => c.Id == x.CoSoThucTapId && c.Ten.Contains(search)));
        }

        return data.OrderByDescending(x => x.Id).ToPagedResultAsync(query, Project);
    }

    public async Task<ThucTapDto> GetByIdAsync(int id) =>
        await Project(_db.Set<ThucTap>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("đăng ký thực tập");

    public async Task<ThucTapDto> CreateAsync(ThucTapRequest request)
    {
        await ValidateReferencesAsync(request.SinhVienId, request.HocKyId,
            request.CoSoThucTapId, request.GiangVienHuongDanId);
        if (await _db.Set<ThucTap>().AnyAsync(x => x.SinhVienId == request.SinhVienId
            && x.HocKyId == request.HocKyId))
            throw AppException.Conflict("Sinh viên đã có đăng ký trong học kỳ này, kể cả đăng ký đã hủy.");

        var item = new ThucTap
        {
            SinhVienId = request.SinhVienId,
            HocKyId = request.HocKyId,
            CoSoThucTapId = request.CoSoThucTapId,
            GiangVienHuongDanId = request.GiangVienHuongDanId
        };
        _db.Set<ThucTap>().Add(item);
        await SaveAsync();
        return await GetByIdAsync(item.Id);
    }

    public async Task<ThucTapDto> UpdateAsync(int id, CapNhatThucTapRequest request)
    {
        var item = await FindAsync(id);
        if (item.TrangThai != TrangThaiThucTap.DangKy)
            throw AppException.Conflict("Chỉ được sửa đăng ký đang có hiệu lực và chưa có kết quả.");
        await ValidateCoSoAsync(request.CoSoThucTapId);
        await ValidateGiangVienAsync(request.GiangVienHuongDanId);

        item.CoSoThucTapId = request.CoSoThucTapId;
        item.GiangVienHuongDanId = request.GiangVienHuongDanId;
        await SaveAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ThucTapDto> HuyAsync(int id)
    {
        var item = await FindAsync(id);
        if (item.TrangThai != TrangThaiThucTap.DangKy)
            throw AppException.Conflict("Chỉ được hủy đăng ký đang có hiệu lực và chưa có kết quả.");
        item.TrangThai = TrangThaiThucTap.DaHuy;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<KetQuaThucTapDto?> GetKetQuaAsync(int id)
    {
        var item = await _db.Set<ThucTap>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("đăng ký thực tập");
        return item.Diem is null ? null : new KetQuaThucTapDto(
            item.Diem.Value, item.NhanXet, item.NgayGhiNhanKetQua);
    }

    public async Task<KetQuaThucTapDto> SetKetQuaAsync(int id, KetQuaThucTapRequest request)
    {
        if (decimal.Round(request.Diem, 2) != request.Diem)
            throw AppException.BadRequest("Điểm thực tập có tối đa 2 chữ số thập phân.");

        var item = await FindAsync(id);
        if (item.TrangThai == TrangThaiThucTap.DaHuy)
            throw AppException.Conflict("Không thể nhập kết quả cho đăng ký đã hủy.");

        item.Diem = request.Diem;
        item.NhanXet = request.NhanXet?.Trim();
        item.NgayGhiNhanKetQua = DateTime.UtcNow;
        item.TrangThai = TrangThaiThucTap.HoanThanh;
        await _db.SaveChangesAsync();
        return new KetQuaThucTapDto(item.Diem.Value, item.NhanXet, item.NgayGhiNhanKetQua);
    }

    private IQueryable<ThucTapDto> Project(IQueryable<ThucTap> data) =>
        from tt in data
        join sv in _db.Set<SinhVien>().AsNoTracking() on tt.SinhVienId equals sv.Id
        join hk in _db.Set<HocKy>().AsNoTracking() on tt.HocKyId equals hk.Id
        join cs in _db.Set<CoSoThucTap>().AsNoTracking() on tt.CoSoThucTapId equals cs.Id
        join gv0 in _db.Set<GiangVien>().AsNoTracking() on tt.GiangVienHuongDanId equals gv0.Id into gvGroup
        from gv in gvGroup.DefaultIfEmpty()
        select new ThucTapDto(tt.Id, tt.SinhVienId, sv.MSSV, sv.HoTen,
            tt.HocKyId, hk.Ten, tt.CoSoThucTapId, cs.Ten,
            tt.GiangVienHuongDanId, gv == null ? null : gv.HoTen,
            tt.TrangThai.ToString(), tt.Diem, tt.NhanXet, tt.NgayGhiNhanKetQua);

    private async Task ValidateReferencesAsync(int sinhVienId, int hocKyId, int coSoId, int? giangVienId)
    {
        var sinhVien = await _db.Set<SinhVien>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sinhVienId)
            ?? throw AppException.NotFound("sinh viên");
        if (sinhVien.TrangThai != TrangThaiSinhVien.DangHoc)
            throw AppException.Conflict("Sinh viên không ở trạng thái đang học.");
        if (!await _db.Set<HocKy>().AsNoTracking().AnyAsync(x => x.Id == hocKyId))
            throw AppException.NotFound("học kỳ");
        await ValidateCoSoAsync(coSoId);
        await ValidateGiangVienAsync(giangVienId);
    }

    private async Task ValidateCoSoAsync(int id)
    {
        var coSo = await _db.Set<CoSoThucTap>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("cơ sở thực tập");
        if (!coSo.DangTiepNhan)
            throw AppException.Conflict("Cơ sở thực tập đã ngừng tiếp nhận.");
    }

    private async Task ValidateGiangVienAsync(int? id)
    {
        if (id is not null && !await _db.Set<GiangVien>().AsNoTracking().AnyAsync(x => x.Id == id))
            throw AppException.NotFound("giảng viên hướng dẫn");
    }

    private async Task<ThucTap> FindAsync(int id) =>
        await _db.Set<ThucTap>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("đăng ký thực tập");

    private async Task SaveAsync()
    {
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw AppException.Conflict("Sinh viên đã có đăng ký trong học kỳ này.");
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            throw AppException.Conflict("Dữ liệu liên quan đã thay đổi. Hãy kiểm tra lại sinh viên, học kỳ, cơ sở và giảng viên.");
        }
    }
}
