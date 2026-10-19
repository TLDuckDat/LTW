using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Services;

public class DiemRenLuyenService(AppDbContext db) : IDiemRenLuyenService
{
    private readonly AppDbContext _db = db;

    public Task<PagedResult<DiemRenLuyenDto>> GetPagedAsync(SinhVienHocKyQuery query)
    {
        var data = _db.Set<DiemRenLuyen>().AsNoTracking();
        if (query.SinhVienId is int sinhVienId) data = data.Where(x => x.SinhVienId == sinhVienId);
        if (query.HocKyId is int hocKyId) data = data.Where(x => x.HocKyId == hocKyId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var sinhVien = _db.SinhVienPhuHop(query.Search);
            data = data.Where(x => sinhVien.Contains(x.SinhVienId));
        }

        return data.OrderByDescending(x => x.HocKyId).ThenBy(x => x.SinhVienId).ToPagedResultAsync(query, Project);
    }

    public async Task<DiemRenLuyenDto> GetByIdAsync(int id) =>
        await Project(_db.Set<DiemRenLuyen>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("điểm rèn luyện");

    public async Task<DiemRenLuyenDto> CreateAsync(DiemRenLuyenRequest request)
    {
        var sinhVien = await _db.GetSinhVienAsync(request.SinhVienId);
        if (sinhVien.TrangThai != TrangThaiSinhVien.DangHoc)
            throw AppException.Conflict(
                $"Sinh viên đang ở trạng thái '{sinhVien.TrangThai}', không thể nhập điểm rèn luyện.");
        await _db.GetHocKyAsync(request.HocKyId);
        if (await _db.Set<DiemRenLuyen>().AnyAsync(x => x.SinhVienId == request.SinhVienId && x.HocKyId == request.HocKyId))
            throw AppException.Conflict("Sinh viên đã có điểm rèn luyện trong học kỳ này.");

        var diem = new DiemRenLuyen
        {
            SinhVienId = request.SinhVienId,
            HocKyId = request.HocKyId,
            Diem = request.Diem,
            XepLoai = StudentAffairsQueries.XepLoaiRenLuyen(request.Diem)
        };
        _db.Add(diem);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw AppException.Conflict("Sinh viên đã có điểm rèn luyện trong học kỳ này.");
        }
        return await GetByIdAsync(diem.Id);
    }

    public async Task<DiemRenLuyenDto> UpdateAsync(int id, CapNhatDiemRenLuyenRequest request)
    {
        var diem = await _db.Set<DiemRenLuyen>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("điểm rèn luyện");
        diem.Diem = request.Diem;
        diem.XepLoai = StudentAffairsQueries.XepLoaiRenLuyen(request.Diem);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        if (await _db.Set<DiemRenLuyen>().Where(x => x.Id == id).ExecuteDeleteAsync() == 0)
            throw AppException.NotFound("điểm rèn luyện");
    }

    private IQueryable<DiemRenLuyenDto> Project(IQueryable<DiemRenLuyen> data) =>
        from d in data
        join sv in _db.Set<SinhVien>().AsNoTracking() on d.SinhVienId equals sv.Id
        join hk in _db.Set<HocKy>().AsNoTracking() on d.HocKyId equals hk.Id
        select new DiemRenLuyenDto(d.Id, sv.Id, sv.MSSV, sv.HoTen, hk.Id, hk.Ten,
            d.Diem, d.XepLoai, d.CreatedAt, d.UpdatedAt);
}
