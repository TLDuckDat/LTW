using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Services;

public class LichHocService(AppDbContext db, IXungDotLichService xungDot) : ILichHocService
{
    /// <summary>Số tiết tối đa trong một ngày.</summary>
    public const int SoTietMotNgay = 12;

    private readonly AppDbContext _db = db;
    private readonly IXungDotLichService _xungDot = xungDot;

    public async Task<IReadOnlyList<LichHocDto>?> GetByLopAsync(int lopHocPhanId)
    {
        if (!await _db.Set<LopHocPhan>().AnyAsync(x => x.Id == lopHocPhanId)) return null;

        return await ProjectToDto(_db.Set<LichHoc>().AsNoTracking().Where(x => x.LopHocPhanId == lopHocPhanId))
            .OrderBy(x => x.Thu).ThenBy(x => x.TietBatDau)
            .ToListAsync();
    }

    // CRS-05 Xếp một buổi học (thứ, tiết, phòng) cho lớp học phần
    public async Task<LichHocDto?> AddAsync(int lopHocPhanId, LichHocRequest request)
    {
        var lop = await _db.Set<LopHocPhan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lopHocPhanId);
        if (lop is null) return null;

        await KiemTraHopLeAsync(lop, request);
        await ChanXungDotAsync(lop, request, null);

        var entity = new LichHoc
        {
            LopHocPhanId = lopHocPhanId,
            Thu = request.Thu,
            TietBatDau = request.TietBatDau,
            SoTiet = request.SoTiet,
            PhongHocId = request.PhongHocId
        };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return await ProjectToDto(_db.Set<LichHoc>().AsNoTracking().Where(x => x.Id == entity.Id)).FirstAsync();
    }

    // CRS-05 Sửa buổi học
    public async Task<LichHocDto?> UpdateAsync(int lopHocPhanId, int lichHocId, LichHocRequest request)
    {
        var lop = await _db.Set<LopHocPhan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lopHocPhanId);
        var entity = await _db.Set<LichHoc>().FirstOrDefaultAsync(x => x.Id == lichHocId && x.LopHocPhanId == lopHocPhanId);
        if (lop is null || entity is null) return null;

        await KiemTraHopLeAsync(lop, request);
        await ChanXungDotAsync(lop, request, lichHocId);

        entity.Thu = request.Thu;
        entity.TietBatDau = request.TietBatDau;
        entity.SoTiet = request.SoTiet;
        entity.PhongHocId = request.PhongHocId;
        await _db.SaveChangesAsync();
        return await ProjectToDto(_db.Set<LichHoc>().AsNoTracking().Where(x => x.Id == lichHocId)).FirstAsync();
    }

    // CRS-06 Kiểm tra xung đột phòng/giảng viên/lớp cho buổi học dự kiến, không lưu dữ liệu
    public async Task<KiemTraXungDotDto?> KiemTraXungDotAsync(int lopHocPhanId, LichHocRequest request, int? lichHocId)
    {
        var lop = await _db.Set<LopHocPhan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lopHocPhanId);
        if (lop is null) return null;

        var xungDot = await _xungDot.TimXungDotAsync(
            lop, lop.GiangVienId, request.PhongHocId, request.Thu, request.TietBatDau, request.SoTiet, lichHocId);
        return new KiemTraXungDotDto(xungDot.Count > 0, xungDot);
    }

    // CRS-05 Xóa buổi học
    public async Task<bool> DeleteAsync(int lopHocPhanId, int lichHocId)
    {
        var entity = await _db.Set<LichHoc>().FirstOrDefaultAsync(x => x.Id == lichHocId && x.LopHocPhanId == lopHocPhanId);
        if (entity is null) return false;

        _db.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>Kiểm tra dữ liệu buổi học tự thân hợp lệ: trạng thái lớp, khung tiết, phòng và sức chứa.</summary>
    private async Task KiemTraHopLeAsync(LopHocPhan lop, LichHocRequest request)
    {
        if (lop.TrangThai is TrangThaiLopHocPhan.DaHuy or TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Lớp học phần đã hủy hoặc kết thúc, không thể xếp lịch.");

        if (request.TietBatDau + request.SoTiet - 1 > SoTietMotNgay)
            throw new InvalidOperationException($"Buổi học vượt quá tiết {SoTietMotNgay} trong ngày.");

        var phong = await _db.Set<PhongHoc>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.PhongHocId)
            ?? throw new InvalidOperationException("Phòng học không tồn tại.");
        if (phong.SucChua < lop.SiSoToiDa)
            throw new InvalidOperationException(
                $"Phòng {phong.Ma} chỉ chứa {phong.SucChua} chỗ, nhỏ hơn sĩ số tối đa của lớp ({lop.SiSoToiDa}).");
    }

    private async Task ChanXungDotAsync(LopHocPhan lop, LichHocRequest request, int? lichHocId)
    {
        var xungDot = await _xungDot.TimXungDotAsync(
            lop, lop.GiangVienId, request.PhongHocId, request.Thu, request.TietBatDau, request.SoTiet, lichHocId);
        if (xungDot.Count > 0)
            throw new InvalidOperationException("Xung đột lịch học: " + string.Join(" ", xungDot.Select(x => x.ThongBao)));
    }

    private IQueryable<LichHocDto> ProjectToDto(IQueryable<LichHoc> q)
        => from l in q
           join p in _db.Set<PhongHoc>() on l.PhongHocId equals p.Id into phong
           from p in phong.DefaultIfEmpty()
           select new LichHocDto(l.Id, l.LopHocPhanId, l.Thu, l.TietBatDau, l.SoTiet, l.PhongHocId, p == null ? null : p.Ma);
}
