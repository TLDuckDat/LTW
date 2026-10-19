using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Courses.Services;

public class LopHocPhanService(AppDbContext db, IXungDotLichService xungDot) : ILopHocPhanService
{
    private readonly AppDbContext _db = db;
    private readonly IXungDotLichService _xungDot = xungDot;

    // CRS-02 Danh sách lớp học phần, lọc theo học kỳ/môn/giảng viên/trạng thái
    public async Task<PagedResult<LopHocPhanDto>> GetPagedAsync(LopHocPhanQuery query)
    {
        var q = _db.Set<LopHocPhan>().AsNoTracking().AsQueryable();

        if (query.HocKyId is not null) q = q.Where(x => x.HocKyId == query.HocKyId);
        if (query.MonHocId is not null) q = q.Where(x => x.MonHocId == query.MonHocId);
        if (query.GiangVienId is not null) q = q.Where(x => x.GiangVienId == query.GiangVienId);
        if (query.TrangThai is not null) q = q.Where(x => x.TrangThai == query.TrangThai);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => x.Ma.Contains(s));
        }

        var total = await q.CountAsync();
        var items = await ProjectToDto(q.OrderBy(x => x.Ma)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize))
            .ToListAsync();

        return new PagedResult<LopHocPhanDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public async Task<LopHocPhanDto?> GetByIdAsync(int id)
        => await ProjectToDto(_db.Set<LopHocPhan>().AsNoTracking().Where(x => x.Id == id)).FirstOrDefaultAsync();

    // CRS-02 Mở lớp học phần trong học kỳ
    public async Task<LopHocPhanDto> CreateAsync(LopHocPhanRequest request)
    {
        var ma = request.Ma.Trim();
        if (!await _db.Set<HocKy>().AnyAsync(x => x.Id == request.HocKyId))
            throw new InvalidOperationException("Học kỳ không tồn tại.");
        if (!await _db.Set<MonHoc>().AnyAsync(x => x.Id == request.MonHocId))
            throw new InvalidOperationException("Môn học không tồn tại.");
        if (await _db.Set<LopHocPhan>().AnyAsync(x => x.Ma == ma))
            throw new InvalidOperationException($"Mã lớp học phần '{ma}' đã tồn tại.");

        var entity = new LopHocPhan
        {
            Ma = ma,
            HocKyId = request.HocKyId,
            MonHocId = request.MonHocId,
            SiSoToiDa = request.SiSoToiDa
        };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(entity.Id))!;
    }

    // CRS-02 Cập nhật mã lớp và sĩ số tối đa
    public async Task<LopHocPhanDto?> UpdateAsync(int id, LopHocPhanUpdateRequest request)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return null;

        if (entity.TrangThai is TrangThaiLopHocPhan.DaHuy or TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Lớp học phần đã hủy hoặc kết thúc, không thể chỉnh sửa.");

        var ma = request.Ma.Trim();
        if (await _db.Set<LopHocPhan>().AnyAsync(x => x.Ma == ma && x.Id != id))
            throw new InvalidOperationException($"Mã lớp học phần '{ma}' đã tồn tại.");

        await KiemTraSiSoToiDaAsync(id, request.SiSoToiDa);

        entity.Ma = ma;
        entity.SiSoToiDa = request.SiSoToiDa;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // CRS-02, REG-07 Chỉ cập nhật sĩ số tối đa của lớp
    public async Task<LopHocPhanDto?> CapNhatSiSoToiDaAsync(int id, int siSoToiDa)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return null;

        if (entity.TrangThai is TrangThaiLopHocPhan.DaHuy or TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Lớp học phần đã hủy hoặc kết thúc, không thể chỉnh sửa.");
        await KiemTraSiSoToiDaAsync(id, siSoToiDa);

        entity.SiSoToiDa = siSoToiDa;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // Sĩ số tối đa không nhỏ hơn số đã đăng ký và không vượt sức chứa của mọi phòng đã xếp cho lớp
    private async Task KiemTraSiSoToiDaAsync(int id, int siSoToiDa)
    {
        var daDangKy = await DemDangKyAsync(id);
        if (siSoToiDa < daDangKy)
            throw new InvalidOperationException($"Sĩ số tối đa không được nhỏ hơn số sinh viên đã đăng ký ({daDangKy}).");

        var sucChuaNhoNhat = await _db.Set<LichHoc>()
            .Where(l => l.LopHocPhanId == id && l.PhongHocId != null)
            .Join(_db.Set<PhongHoc>(), l => l.PhongHocId, p => p.Id, (l, p) => (int?)p.SucChua)
            .MinAsync();
        if (sucChuaNhoNhat is not null && siSoToiDa > sucChuaNhoNhat)
            throw new InvalidOperationException($"Sĩ số tối đa vượt sức chứa phòng đã xếp ({sucChuaNhoNhat}).");
    }

    // CRS-02 Xóa lớp học phần chưa có sinh viên đăng ký (lịch học xóa theo lớp)
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return false;

        if (await _db.Set<DangKyHocPhan>().AnyAsync(x => x.LopHocPhanId == id))
            throw new InvalidOperationException("Lớp học phần đã có lượt đăng ký, không thể xóa (hãy hủy lớp).");

        _db.RemoveRange(await _db.Set<LichHoc>().Where(x => x.LopHocPhanId == id).ToListAsync());
        _db.Remove(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa lớp học phần đang có dữ liệu liên quan.");
        }
        return true;
    }

    // CRS-03 Phân công (hoặc bỏ phân công) giảng viên cho lớp, chặn khi giảng viên trùng giờ giảng
    public async Task<LopHocPhanDto?> PhanCongGiangVienAsync(int id, PhanCongGiangVienRequest request)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return null;

        if (entity.TrangThai is TrangThaiLopHocPhan.DaHuy or TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Lớp học phần đã hủy hoặc kết thúc, không thể phân công giảng viên.");

        if (request.GiangVienId is { } giangVienId)
        {
            if (!await _db.Set<GiangVien>().AnyAsync(x => x.Id == giangVienId))
                throw new InvalidOperationException("Giảng viên không tồn tại.");

            var xungDot = await _xungDot.TimXungDotGiangVienAsync(entity, giangVienId);
            if (xungDot.Count > 0)
                throw new InvalidOperationException("Giảng viên bị trùng lịch giảng: " + string.Join(" ", xungDot.Select(x => x.ThongBao)));
        }

        entity.GiangVienId = request.GiangVienId;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // CRS-08 Bắt đầu học: lớp ngừng nhận đăng ký/hủy đăng ký, từ đây mới được nhập điểm
    public async Task<LopHocPhanDto?> BatDauHocAsync(int id)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return null;

        if (entity.TrangThai != TrangThaiLopHocPhan.MoDangKy)
            throw new InvalidOperationException("Chỉ lớp học phần đang mở đăng ký mới chuyển sang đang học được.");
        if (await DemDangKyAsync(id) == 0)
            throw new InvalidOperationException("Lớp học phần chưa có sinh viên đăng ký, hãy hủy lớp thay vì bắt đầu học.");

        entity.TrangThai = TrangThaiLopHocPhan.DangHoc;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // CRS-08 Kết thúc lớp: từ đây mới được duyệt, khóa bảng điểm và phúc khảo
    public async Task<LopHocPhanDto?> KetThucAsync(int id)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return null;

        if (entity.TrangThai != TrangThaiLopHocPhan.DangHoc)
            throw new InvalidOperationException("Chỉ lớp học phần đang học mới kết thúc được.");

        entity.TrangThai = TrangThaiLopHocPhan.DaKetThuc;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // CRS-07 Hủy lớp học phần: hủy các lượt đăng ký còn hiệu lực, giữ lại dữ liệu để tra cứu
    public async Task<LopHocPhanDto?> HuyAsync(int id)
    {
        var entity = await _db.Set<LopHocPhan>().FindAsync(id);
        if (entity is null) return null;

        if (entity.TrangThai == TrangThaiLopHocPhan.DaHuy)
            throw new InvalidOperationException("Lớp học phần đã được hủy trước đó.");
        if (entity.TrangThai == TrangThaiLopHocPhan.DaKetThuc)
            throw new InvalidOperationException("Lớp học phần đã kết thúc, không thể hủy.");
        if (await CoDiemAsync(id))
            throw new InvalidOperationException("Lớp học phần đã có điểm, không thể hủy.");

        var dangKy = await _db.Set<DangKyHocPhan>()
            .Where(x => x.LopHocPhanId == id && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .ToListAsync();
        foreach (var d in dangKy) d.TrangThai = TrangThaiDangKy.DaHuy;

        entity.TrangThai = TrangThaiLopHocPhan.DaHuy;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // CRS-07 Gộp lớp: chuyển sinh viên sang lớp đích rồi hủy lớp nguồn
    public async Task<GopLopResultDto?> GopAsync(int id, GopLopRequest request)
    {
        var nguon = await _db.Set<LopHocPhan>().FindAsync(id);
        var dich = await _db.Set<LopHocPhan>().FindAsync(request.LopDichId);
        if (nguon is null || dich is null) return null;

        if (nguon.Id == dich.Id)
            throw new InvalidOperationException("Không thể gộp một lớp vào chính nó.");
        if (nguon.TrangThai != TrangThaiLopHocPhan.MoDangKy || dich.TrangThai != TrangThaiLopHocPhan.MoDangKy)
            throw new InvalidOperationException("Chỉ gộp được hai lớp học phần đang mở đăng ký.");
        if (nguon.MonHocId != dich.MonHocId)
            throw new InvalidOperationException("Hai lớp học phần không cùng môn học.");
        if (nguon.HocKyId != dich.HocKyId)
            throw new InvalidOperationException("Hai lớp học phần không cùng học kỳ.");
        if (await CoDiemAsync(nguon.Id))
            throw new InvalidOperationException("Lớp nguồn đã có điểm, không thể gộp.");

        var dangKyNguon = await _db.Set<DangKyHocPhan>()
            .Where(x => x.LopHocPhanId == nguon.Id && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .ToListAsync();
        // Cặp (sinh viên, lớp) là duy nhất nên mỗi sinh viên ở lớp nguồn có ba trường hợp:
        // đã đăng ký còn hiệu lực ở lớp đích (trùng), có dòng đã hủy ở lớp đích (dùng lại dòng đó),
        // hoặc chưa có dòng nào (chuyển thẳng dòng sang lớp đích)
        var dongLopDich = (await _db.Set<DangKyHocPhan>()
                .Where(x => x.LopHocPhanId == dich.Id)
                .ToListAsync())
            .ToDictionary(x => x.SinhVienId);
        var trung = dangKyNguon
            .Where(x => dongLopDich.TryGetValue(x.SinhVienId, out var d) && d.TrangThai == TrangThaiDangKy.DaDangKy)
            .ToList();
        var canChuyen = dangKyNguon.Except(trung).ToList();

        var daDangKyDich = await DemDangKyAsync(dich.Id);
        if (daDangKyDich + canChuyen.Count > dich.SiSoToiDa)
            throw new InvalidOperationException(
                $"Lớp đích không đủ chỗ: còn {dich.SiSoToiDa - daDangKyDich} chỗ, cần chuyển {canChuyen.Count} sinh viên.");

        foreach (var d in canChuyen)
        {
            if (dongLopDich.TryGetValue(d.SinhVienId, out var dongCu))
            {
                dongCu.TrangThai = TrangThaiDangKy.DaDangKy;
                dongCu.LoaiDangKy = d.LoaiDangKy;
                d.TrangThai = TrangThaiDangKy.DaHuy;
            }
            else
            {
                d.LopHocPhanId = dich.Id;
            }
        }
        foreach (var d in trung) d.TrangThai = TrangThaiDangKy.DaHuy;
        nguon.TrangThai = TrangThaiLopHocPhan.DaHuy;
        await _db.SaveChangesAsync();

        return new GopLopResultDto((await GetByIdAsync(dich.Id))!, canChuyen.Count, trung.Count);
    }

    private Task<bool> CoDiemAsync(int lopHocPhanId)
        => (from dk in _db.Set<DangKyHocPhan>()
            join d in _db.Set<DiemHocPhan>() on dk.Id equals d.DangKyHocPhanId
            where dk.LopHocPhanId == lopHocPhanId
            select d.Id).AnyAsync();

    private Task<int> DemDangKyAsync(int lopHocPhanId)
        => _db.Set<DangKyHocPhan>()
            .CountAsync(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy);

    private IQueryable<LopHocPhanDto> ProjectToDto(IQueryable<LopHocPhan> q)
        => from l in q
           join m in _db.Set<MonHoc>() on l.MonHocId equals m.Id
           select new LopHocPhanDto(
               l.Id, l.Ma, l.HocKyId, l.MonHocId, m.Ma, m.Ten, l.GiangVienId, l.SiSoToiDa,
               _db.Set<DangKyHocPhan>().Count(d => d.LopHocPhanId == l.Id && d.TrangThai == TrangThaiDangKy.DaDangKy),
               l.TrangThai, l.CreatedAt, l.UpdatedAt);
}
