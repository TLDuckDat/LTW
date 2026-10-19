using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Grades.Services;

public class SinhVienLopHocPhanService(AppDbContext db) : ISinhVienLopHocPhanService
{
    private readonly AppDbContext _db = db;

    public async Task<PagedResult<SinhVienTrongLopDto>> GetDanhSachAsync(int lopHocPhanId, PagedQuery query)
    {
        await _db.GetLopHocPhanAsync(lopHocPhanId);

        var dangKy = _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .Join(_db.Set<SinhVien>().AsNoTracking(), dk => dk.SinhVienId, sv => sv.Id, (dk, sv) => new { dk, sv });
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            dangKy = dangKy.Where(x => x.sv.MSSV.Contains(search) || x.sv.HoTen.Contains(search));
        }

        return await dangKy.OrderBy(x => x.sv.MSSV)
            .Select(x => new SinhVienTrongLopDto(x.dk.Id, x.sv.MSSV, x.sv.HoTen, x.dk.LoaiDangKy.ToString()))
            .ToPagedResultAsync(query);
    }

    public async Task<DangKyHocPhanDto> ThemSinhVienAsync(int lopHocPhanId, ThemSinhVienRequest request)
    {
        var lop = await _db.GetLopHocPhanAsync(lopHocPhanId);
        var sinhVien = await _db.Set<SinhVien>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SinhVienId)
            ?? throw AppException.NotFound("sinh viên");

        if (lop.TrangThai != TrangThaiLopHocPhan.MoDangKy)
            throw AppException.Conflict("Lớp học phần không còn cho phép đăng ký.");
        if (sinhVien.TrangThai != TrangThaiSinhVien.DangHoc)
            throw AppException.Conflict("Sinh viên không ở trạng thái đang học.");

        var dangKy = await _db.Set<DangKyHocPhan>()
            .FirstOrDefaultAsync(x => x.SinhVienId == request.SinhVienId && x.LopHocPhanId == lopHocPhanId);
        if (dangKy?.TrangThai == TrangThaiDangKy.DaDangKy)
            throw AppException.Conflict("Sinh viên đã có trong lớp học phần này.");
        if (await DaDangKyMonTrongHocKyAsync(request.SinhVienId, lop))
            throw AppException.Conflict("Sinh viên đã đăng ký môn học này ở lớp khác trong cùng học kỳ.");
        if (await LopDaDuSiSoAsync(lop))
            throw AppException.Conflict("Lớp học phần đã đủ sĩ số.");

        var loaiDangKy = await XacDinhLoaiDangKyAsync(request.SinhVienId, lop);
        if (dangKy is null)
        {
            dangKy = new DangKyHocPhan
            {
                SinhVienId = request.SinhVienId, LopHocPhanId = lopHocPhanId, LoaiDangKy = loaiDangKy
            };
            _db.Add(dangKy);
        }
        else
        {
            // Lượt đăng ký cũ đã hủy: dùng lại vì cặp (SinhVienId, LopHocPhanId) là unique
            if (await CoDiemAsync(dangKy.Id))
                throw AppException.Conflict("Lượt đăng ký cũ đã có điểm, không thể đăng ký lại.");
            dangKy.TrangThai = TrangThaiDangKy.DaDangKy;
            dangKy.LoaiDangKy = loaiDangKy;
        }

        await _db.SaveChangesAsync();
        return ToDto(dangKy);
    }

    public async Task<DangKyHocPhanDto> ChuyenLopAsync(int lopHocPhanId, int dangKyHocPhanId, ChuyenLopRequest request)
    {
        var lopCu = await _db.GetLopHocPhanAsync(lopHocPhanId);
        var lopMoi = await _db.GetLopHocPhanAsync(request.LopHocPhanMoiId, "lớp học phần mới");
        var dangKy = await FindDangKyTrongLopAsync(lopHocPhanId, dangKyHocPhanId);

        KiemTraLopMoDangKy(lopCu);
        if (dangKy.TrangThai != TrangThaiDangKy.DaDangKy)
            throw AppException.Conflict("Lượt đăng ký đã bị hủy, không thể chuyển lớp.");
        if (lopMoi.TrangThai != TrangThaiLopHocPhan.MoDangKy)
            throw AppException.Conflict("Lớp học phần mới không còn nhận đăng ký.");
        if (lopMoi.MonHocId != lopCu.MonHocId)
            throw AppException.Conflict("Lớp học phần mới không cùng môn học.");
        // Đổi học kỳ sẽ làm sai dữ liệu tính GPA, nên chỉ cho chuyển trong cùng học kỳ
        if (lopMoi.HocKyId != lopCu.HocKyId)
            throw AppException.Conflict("Lớp học phần mới không cùng học kỳ.");
        if (await CoDiemAsync(dangKy.Id))
            throw AppException.Conflict("Lượt đăng ký đã có điểm, không thể chuyển lớp.");

        // Lượt đăng ký ở lớp mới (kể cả đã hủy) đều làm trùng cặp unique, nên chặn luôn
        if (await _db.Set<DangKyHocPhan>().AnyAsync(x => x.SinhVienId == dangKy.SinhVienId && x.LopHocPhanId == lopMoi.Id))
            throw AppException.Conflict("Sinh viên đã có lượt đăng ký ở lớp học phần mới.");
        // Bỏ qua chính lượt đang chuyển, chỉ chặn khi còn lượt khác cùng môn cùng học kỳ
        if (await DaDangKyMonTrongHocKyAsync(dangKy.SinhVienId, lopMoi, boQuaDangKyId: dangKy.Id))
            throw AppException.Conflict("Sinh viên đã đăng ký môn học này ở lớp khác trong cùng học kỳ.");
        if (await LopDaDuSiSoAsync(lopMoi))
            throw AppException.Conflict("Lớp học phần mới đã đủ sĩ số.");

        dangKy.LopHocPhanId = lopMoi.Id;
        await _db.SaveChangesAsync();
        return ToDto(dangKy);
    }

    public async Task HuyDangKyAsync(int lopHocPhanId, int dangKyHocPhanId)
    {
        var lop = await _db.GetLopHocPhanAsync(lopHocPhanId);
        var dangKy = await FindDangKyTrongLopAsync(lopHocPhanId, dangKyHocPhanId);

        KiemTraLopMoDangKy(lop);
        if (dangKy.TrangThai == TrangThaiDangKy.DaHuy)
            throw AppException.Conflict("Lượt đăng ký này đã được hủy trước đó.");
        if (await CoDiemAsync(dangKy.Id))
            throw AppException.Conflict("Lượt đăng ký đã có điểm, không thể hủy.");

        dangKy.TrangThai = TrangThaiDangKy.DaHuy;
        await _db.SaveChangesAsync();
    }

    private async Task<DangKyHocPhan> FindDangKyTrongLopAsync(int lopHocPhanId, int dangKyHocPhanId) =>
        await _db.Set<DangKyHocPhan>()
            .FirstOrDefaultAsync(x => x.Id == dangKyHocPhanId && x.LopHocPhanId == lopHocPhanId)
            ?? throw AppException.NotFound("lượt đăng ký trong lớp học phần này");

    // Chuyển lớp và hủy đăng ký chỉ làm được khi lớp còn đang mở đăng ký
    private static void KiemTraLopMoDangKy(LopHocPhan lop)
    {
        if (lop.TrangThai == TrangThaiLopHocPhan.MoDangKy) return;

        var lyDo = lop.TrangThai switch
        {
            TrangThaiLopHocPhan.DangHoc => "đã bắt đầu học",
            TrangThaiLopHocPhan.DaKetThuc => "đã kết thúc",
            _ => "đã bị hủy"
        };
        throw AppException.Conflict($"Lớp học phần {lyDo}, không thể chuyển hoặc hủy đăng ký.");
    }

    // SiSoToiDa = 0 nghĩa là lớp không giới hạn sĩ số
    private async Task<bool> LopDaDuSiSoAsync(LopHocPhan lop)
    {
        if (lop.SiSoToiDa <= 0) return false;

        var siSoHienTai = await _db.Set<DangKyHocPhan>()
            .CountAsync(x => x.LopHocPhanId == lop.Id && x.TrangThai == TrangThaiDangKy.DaDangKy);
        return siSoHienTai >= lop.SiSoToiDa;
    }

    // Một sinh viên chỉ được có một lượt đăng ký còn hiệu lực cho mỗi môn trong một học kỳ
    private Task<bool> DaDangKyMonTrongHocKyAsync(int sinhVienId, LopHocPhan lop, int? boQuaDangKyId = null) =>
        (from dk in _db.Set<DangKyHocPhan>()
         join lhp in _db.Set<LopHocPhan>() on dk.LopHocPhanId equals lhp.Id
         where dk.SinhVienId == sinhVienId && dk.TrangThai == TrangThaiDangKy.DaDangKy
               && lhp.MonHocId == lop.MonHocId && lhp.HocKyId == lop.HocKyId
               && dk.Id != boQuaDangKyId
         select dk.Id).AnyAsync();

    // Xác định loại đăng ký theo các lượt học môn này ở những học kỳ trước:
    // chưa học -> HocMoi, đã có lượt đạt (điểm đã khóa >= điểm đạt) -> HocCaiThien, còn lại -> HocLai
    private async Task<LoaiDangKy> XacDinhLoaiDangKyAsync(int sinhVienId, LopHocPhan lop)
    {
        var ngayBatDauHocKy = await _db.Set<HocKy>()
            .Where(x => x.Id == lop.HocKyId)
            .Select(x => x.NgayBatDau)
            .FirstAsync();

        var ketQuaTruoc = await (
            from dk in _db.Set<DangKyHocPhan>()
            join lhp in _db.Set<LopHocPhan>() on dk.LopHocPhanId equals lhp.Id
            join hk in _db.Set<HocKy>() on lhp.HocKyId equals hk.Id
            where dk.SinhVienId == sinhVienId && dk.TrangThai == TrangThaiDangKy.DaDangKy
                  && lhp.MonHocId == lop.MonHocId && hk.NgayBatDau < ngayBatDauHocKy
            join diem in _db.Set<DiemHocPhan>() on dk.Id equals diem.DangKyHocPhanId into nhomDiem
            from diem in nhomDiem.DefaultIfEmpty()
            select diem != null && diem.DaKhoa && diem.DiemTongKet >= GradeCalculator.DiemDat).ToListAsync();

        if (ketQuaTruoc.Count == 0) return LoaiDangKy.HocMoi;
        return ketQuaTruoc.Any(dat => dat) ? LoaiDangKy.HocCaiThien : LoaiDangKy.HocLai;
    }

    private Task<bool> CoDiemAsync(int dangKyHocPhanId) =>
        _db.Set<DiemHocPhan>().AnyAsync(x => x.DangKyHocPhanId == dangKyHocPhanId);

    private static DangKyHocPhanDto ToDto(DangKyHocPhan x) =>
        new(x.Id, x.SinhVienId, x.LopHocPhanId, x.LoaiDangKy.ToString(), x.TrangThai.ToString());
}
