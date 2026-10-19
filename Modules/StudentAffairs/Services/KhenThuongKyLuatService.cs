using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Services;

public class KhenThuongKyLuatService(AppDbContext db) : IKhenThuongKyLuatService
{
    private readonly AppDbContext _db = db;

    public Task<PagedResult<KhenThuongKyLuatDto>> GetPagedAsync(KhenThuongKyLuatQuery query)
    {
        var data = _db.Set<KhenThuongKyLuat>().AsNoTracking();
        if (query.SinhVienId is int sinhVienId) data = data.Where(x => x.SinhVienId == sinhVienId);
        if (query.Loai is { } loai)
        {
            // Query string dạng số (ví dụ ?loai=9) vẫn bind được vào enum, nên phải kiểm tra lại
            if (!Enum.IsDefined(loai))
                throw AppException.BadRequest("Loại quyết định không hợp lệ.");
            data = data.Where(x => x.Loai == loai);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var sinhVien = _db.SinhVienPhuHop(query.Search);
            data = data.Where(x => sinhVien.Contains(x.SinhVienId));
        }

        return data.OrderByDescending(x => x.NgayQuyetDinh).ThenByDescending(x => x.Id)
            .ToPagedResultAsync(query, Project);
    }

    public async Task<KhenThuongKyLuatDto> GetByIdAsync(int id) =>
        await Project(_db.Set<KhenThuongKyLuat>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync()
            ?? throw AppException.NotFound("quyết định khen thưởng/kỷ luật");

    public async Task<KhenThuongKyLuatDto> CreateAsync(KhenThuongKyLuatRequest request)
    {
        var quyetDinh = new KhenThuongKyLuat();
        await ApplyAsync(quyetDinh, request);
        _db.Add(quyetDinh);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(quyetDinh.Id);
    }

    public async Task<KhenThuongKyLuatDto> UpdateAsync(int id, KhenThuongKyLuatRequest request)
    {
        var quyetDinh = await _db.Set<KhenThuongKyLuat>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("quyết định khen thưởng/kỷ luật");
        await ApplyAsync(quyetDinh, request);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        if (await _db.Set<KhenThuongKyLuat>().Where(x => x.Id == id).ExecuteDeleteAsync() == 0)
            throw AppException.NotFound("quyết định khen thưởng/kỷ luật");
    }

    private async Task ApplyAsync(KhenThuongKyLuat quyetDinh, KhenThuongKyLuatRequest request)
    {
        var loai = request.Loai ?? throw AppException.BadRequest("Loại quyết định là bắt buộc.");
        if (!Enum.IsDefined(loai))
            throw AppException.BadRequest("Loại quyết định không hợp lệ.");
        var noiDung = request.NoiDung.Trim();
        if (noiDung.Length == 0)
            throw AppException.BadRequest("Nội dung quyết định không được để trống.");
        await _db.GetSinhVienAsync(request.SinhVienId);

        quyetDinh.SinhVienId = request.SinhVienId;
        quyetDinh.Loai = loai;
        quyetDinh.SoQuyetDinh = string.IsNullOrWhiteSpace(request.SoQuyetDinh) ? null : request.SoQuyetDinh.Trim();
        quyetDinh.NgayQuyetDinh = request.NgayQuyetDinh ?? throw AppException.BadRequest("Ngày quyết định là bắt buộc.");
        quyetDinh.NoiDung = noiDung;
    }

    private IQueryable<KhenThuongKyLuatDto> Project(IQueryable<KhenThuongKyLuat> data) =>
        from qd in data
        join sv in _db.Set<SinhVien>().AsNoTracking() on qd.SinhVienId equals sv.Id
        select new KhenThuongKyLuatDto(qd.Id, sv.Id, sv.MSSV, sv.HoTen, qd.Loai.ToString(),
            qd.SoQuyetDinh, qd.NgayQuyetDinh, qd.NoiDung, qd.CreatedAt, qd.UpdatedAt);
}
