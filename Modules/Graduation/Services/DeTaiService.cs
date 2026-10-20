using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Services;

public class DeTaiService(AppDbContext db) : IDeTaiService
{
    private readonly AppDbContext _db = db;

    public Task<PagedResult<DeTaiDto>> GetPagedAsync(PagedQuery query)
    {
        var data = _db.Set<DeTaiKhoaLuan>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            data = data.Where(x => x.TenDeTai.Contains(search));
        }

        return data.OrderByDescending(x => x.Id).ToPagedResultAsync(query, Project);
    }

    public async Task<DeTaiDto> GetByIdAsync(int id) =>
        await Project(_db.Set<DeTaiKhoaLuan>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("đề tài");

    public async Task<DeTaiDto> CreateAsync(DeTaiRequest request)
    {
        var item = new DeTaiKhoaLuan { TenDeTai = request.TenDeTai.Trim() };
        _db.Set<DeTaiKhoaLuan>().Add(item);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(item.Id);
    }

    public async Task<DeTaiDto> UpdateAsync(int id, DeTaiRequest request)
    {
        var item = await FindAsync(id);
        item.TenDeTai = request.TenDeTai.Trim();
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<DeTaiDto> DangKyAsync(int id, DangKyDeTaiRequest request)
    {
        var item = await _db.Set<DeTaiKhoaLuan>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("đề tài");
        if (item.SinhVienId is not null)
            throw AppException.Conflict("Đề tài đã có sinh viên nhận.");

        var sinhVien = await _db.Set<SinhVien>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SinhVienId)
            ?? throw AppException.NotFound("sinh viên");
        if (sinhVien.TrangThai != TrangThaiSinhVien.DangHoc)
            throw AppException.Conflict("Sinh viên không ở trạng thái đang học.");
        if (await _db.Set<DeTaiKhoaLuan>().AnyAsync(x => x.SinhVienId == request.SinhVienId))
            throw AppException.Conflict("Sinh viên đã đăng ký một đề tài khác.");

        try
        {
            // UPDATE có điều kiện để hai yêu cầu cùng lúc không ghi đè sinh viên của nhau.
            var updated = await _db.Set<DeTaiKhoaLuan>()
                .Where(x => x.Id == id && x.SinhVienId == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.SinhVienId, request.SinhVienId)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
            if (updated == 0)
                throw AppException.Conflict("Đề tài đã có sinh viên nhận.");
        }
        catch (SqlException ex) when (ex.IsUniqueViolation())
        {
            throw AppException.Conflict("Sinh viên đã đăng ký một đề tài khác.");
        }
        catch (SqlException ex) when (ex.IsForeignKeyViolation())
        {
            throw AppException.Conflict("Sinh viên không còn tồn tại. Hãy kiểm tra lại.");
        }

        return await GetByIdAsync(id);
    }

    public async Task<DeTaiDto> PhanCongGiangVienAsync(int id, PhanCongHuongDanRequest request)
    {
        var item = await FindAsync(id);
        if (!await _db.Set<GiangVien>().AsNoTracking()
            .AnyAsync(x => x.Id == request.GiangVienHuongDanId))
            throw AppException.NotFound("giảng viên hướng dẫn");

        item.GiangVienHuongDanId = request.GiangVienHuongDanId;
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            throw AppException.Conflict("Giảng viên không còn tồn tại. Hãy kiểm tra lại.");
        }
        return await GetByIdAsync(id);
    }

    private IQueryable<DeTaiDto> Project(IQueryable<DeTaiKhoaLuan> data) =>
        from dt in data
        join sv0 in _db.Set<SinhVien>().AsNoTracking() on dt.SinhVienId equals sv0.Id into sinhVienGroup
        from sv in sinhVienGroup.DefaultIfEmpty()
        join gv0 in _db.Set<GiangVien>().AsNoTracking() on dt.GiangVienHuongDanId equals gv0.Id into giangVienGroup
        from gv in giangVienGroup.DefaultIfEmpty()
        select new DeTaiDto(dt.Id, dt.TenDeTai, dt.SinhVienId,
            sv == null ? null : sv.MSSV, sv == null ? null : sv.HoTen,
            dt.GiangVienHuongDanId, gv == null ? null : gv.HoTen,
            dt.CreatedAt, dt.UpdatedAt);

    private async Task<DeTaiKhoaLuan> FindAsync(int id) =>
        await _db.Set<DeTaiKhoaLuan>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("đề tài");
}
