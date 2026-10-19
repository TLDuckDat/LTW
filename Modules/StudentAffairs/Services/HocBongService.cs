using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Academic.Services;
using StudentManagement.Modules.Grades.Services;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Services;

public class HocBongService(AppDbContext db, IBangDiemService bangDiem) : IHocBongService
{
    private const string TenHocBong = "Học bổng khuyến khích học tập";

    // Giá trị mặc định khi chưa cấu hình quy chế
    private const int DiemRenLuyenToiThieuMacDinh = 80;
    private const decimal GpaToiThieuMacDinh = 2.5m;
    private const decimal SoTienMacDinh = 1_000_000m;

    private readonly AppDbContext _db = db;
    private readonly IBangDiemService _bangDiem = bangDiem;

    public Task<PagedResult<HocBongDto>> GetPagedAsync(SinhVienHocKyQuery query)
    {
        var data = _db.Set<HocBong>().AsNoTracking();
        if (query.SinhVienId is int sinhVienId) data = data.Where(x => x.SinhVienId == sinhVienId);
        if (query.HocKyId is int hocKyId) data = data.Where(x => x.HocKyId == hocKyId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var sinhVien = _db.SinhVienPhuHop(query.Search);
            data = data.Where(x => sinhVien.Contains(x.SinhVienId));
        }

        return data.OrderByDescending(x => x.HocKyId).ThenBy(x => x.SinhVienId).ToPagedResultAsync(query, Project);
    }

    public async Task<XetHocBongResultDto> XetHocBongAsync(int hocKyId)
    {
        var hocKy = await _db.GetHocKyAsync(hocKyId);
        var diemToiThieu = await _db.GetThamSoNguyenAsync(MaQuyChe.HocBongDiemRenLuyenToiThieu, DiemRenLuyenToiThieuMacDinh);
        var gpaToiThieu = await _db.GetThamSoAsync(MaQuyChe.HocBongGpaToiThieu, GpaToiThieuMacDinh);
        var soTien = await _db.GetThamSoAsync(MaQuyChe.HocBongSoTien, SoTienMacDinh);

        // Ứng viên: sinh viên đã có điểm rèn luyện trong học kỳ
        var ungVien = await (
                from d in _db.Set<DiemRenLuyen>().AsNoTracking()
                join sv in _db.Set<SinhVien>().AsNoTracking() on d.SinhVienId equals sv.Id
                where d.HocKyId == hocKyId
                orderby sv.MSSV
                select new { sv.Id, sv.MSSV, sv.HoTen, sv.TrangThai, d.Diem })
            .ToListAsync();
        var ids = ungVien.Select(x => x.Id).ToList();

        var biKyLuat = (await _db.Set<KhenThuongKyLuat>().AsNoTracking()
                .Where(x => ids.Contains(x.SinhVienId) && x.Loai == LoaiKhenThuongKyLuat.KyLuat
                            && x.NgayQuyetDinh >= hocKy.NgayBatDau && x.NgayQuyetDinh <= hocKy.NgayKetThuc)
                .Select(x => x.SinhVienId)
                .ToListAsync())
            .ToHashSet();
        var daCo = (await _db.Set<HocBong>().AsNoTracking()
                .Where(x => x.HocKyId == hocKyId && x.Ten == TenHocBong)
                .Select(x => x.SinhVienId)
                .ToListAsync())
            .ToHashSet();

        // Chỉ tính bảng điểm cho người đã qua các điều kiện rẻ hơn
        var canXetGpa = ungVien
            .Where(x => x.TrangThai == TrangThaiSinhVien.DangHoc && x.Diem >= diemToiThieu && !biKyLuat.Contains(x.Id))
            .Select(x => x.Id)
            .ToArray();
        var bangDiem = await _bangDiem.GetBangDiemNhieuSinhVienAsync(canXetGpa);

        var khongDat = new List<XetHocBongKhongDatDto>();
        var moiCap = new List<HocBong>();
        var soDaCoTruocDo = 0;
        foreach (var sv in ungVien)
        {
            var lyDo = new List<string>();
            if (sv.TrangThai != TrangThaiSinhVien.DangHoc)
                lyDo.Add($"Sinh viên đang ở trạng thái {sv.TrangThai}.");
            if (sv.Diem < diemToiThieu)
                lyDo.Add($"Điểm rèn luyện {sv.Diem} dưới mức {diemToiThieu}.");
            if (biKyLuat.Contains(sv.Id))
                lyDo.Add("Bị kỷ luật trong học kỳ.");
            if (lyDo.Count == 0)
            {
                var ketQua = bangDiem.GetValueOrDefault(sv.Id)?.DanhSachHocKy.FirstOrDefault(x => x.HocKyId == hocKyId);
                if (ketQua?.Gpa is not { } gpa)
                    lyDo.Add("Chưa có GPA học kỳ từ điểm đã khóa.");
                else if (ketQua.GpaTamTinh)
                    lyDo.Add("GPA học kỳ còn tạm tính vì còn học phần chưa khóa điểm.");
                else if (gpa < gpaToiThieu)
                    lyDo.Add($"GPA học kỳ {gpa:0.00} dưới mức {gpaToiThieu:0.00}.");
            }

            if (lyDo.Count > 0)
            {
                khongDat.Add(new XetHocBongKhongDatDto(sv.Id, sv.MSSV, sv.HoTen, lyDo));
                continue;
            }
            if (daCo.Contains(sv.Id))
            {
                soDaCoTruocDo++;
                continue;
            }
            moiCap.Add(new HocBong { SinhVienId = sv.Id, HocKyId = hocKyId, Ten = TenHocBong, SoTien = soTien });
        }

        _db.AddRange(moiCap);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw AppException.Conflict("Học bổng của học kỳ đang được xét đồng thời, hãy thử lại.");
        }

        var moiCapIds = moiCap.Select(x => x.Id).ToList();
        var danhSachMoiCap = await Project(_db.Set<HocBong>().AsNoTracking().Where(x => moiCapIds.Contains(x.Id)))
            .ToListAsync();
        return new XetHocBongResultDto(
            hocKy.Id, hocKy.Ten, TenHocBong, diemToiThieu, gpaToiThieu, soTien,
            ungVien.Count, moiCap.Count, soDaCoTruocDo, danhSachMoiCap, khongDat);
    }

    public async Task DeleteAsync(int id)
    {
        if (await _db.Set<HocBong>().Where(x => x.Id == id).ExecuteDeleteAsync() == 0)
            throw AppException.NotFound("học bổng");
    }

    private IQueryable<HocBongDto> Project(IQueryable<HocBong> data) =>
        from hb in data
        join sv in _db.Set<SinhVien>().AsNoTracking() on hb.SinhVienId equals sv.Id
        join hk in _db.Set<HocKy>().AsNoTracking() on hb.HocKyId equals hk.Id
        select new HocBongDto(hb.Id, sv.Id, sv.MSSV, sv.HoTen, hk.Id, hk.Ten, hb.Ten, hb.SoTien, hb.CreatedAt);
}
