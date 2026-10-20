using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Services;

public class GraduationService(AppDbContext db, IDieuKienTotNghiepService dieuKien, NhatKyContext nhatKy)
    : IGraduationService
{
    private readonly AppDbContext _db = db;
    private readonly NhatKyContext _nhatKy = nhatKy;
    private readonly IDieuKienTotNghiepService _dieuKien = dieuKien;

    public async Task<XetTotNghiepDto> XetTotNghiepAsync(int sinhVienId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        // Khóa sinh viên đến hết giao dịch để hai yêu cầu xét cùng lúc không tạo hai kết quả.
        var students = await _db.Set<SinhVien>()
            .FromSqlInterpolated($"SELECT * FROM dbo.SinhVien WITH (UPDLOCK, ROWLOCK) WHERE Id = {sinhVienId}")
            .ToListAsync();
        var sv = students.SingleOrDefault()
            ?? throw AppException.NotFound("sinh viên");
        var last = await _db.Set<XetTotNghiep>()
            .Where(x => x.SinhVienId == sinhVienId)
            .OrderByDescending(x => x.Id).FirstOrDefaultAsync();

        if (last?.KetQua == KetQuaXetTotNghiep.DuDieuKien)
        {
            if (sv.TrangThai != TrangThaiSinhVien.DaTotNghiep)
            {
                sv.TrangThai = TrangThaiSinhVien.DaTotNghiep;
                await _db.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            return await GetRecordAsync(last.Id);
        }
        if (sv.TrangThai == TrangThaiSinhVien.DaTotNghiep)
            throw AppException.Conflict(
                "Sinh viên đã tốt nghiệp nhưng chưa có kết quả xét được công nhận. Cần kiểm tra dữ liệu cũ.");

        var check = await _dieuKien.GetBySinhVienIdAsync(sinhVienId);
        var ketQua = check.KetLuan switch
        {
            KetLuanTotNghiep.DuDieuKien => KetQuaXetTotNghiep.DuDieuKien,
            KetLuanTotNghiep.KhongDuDieuKien => KetQuaXetTotNghiep.KhongDuDieuKien,
            _ => KetQuaXetTotNghiep.ChoXet
        };
        var lyDo = ketQua == KetQuaXetTotNghiep.DuDieuKien ? null
            : string.Join("; ", check.DanhSachDieuKien
                .Where(x => x.TrangThai != TrangThaiDieuKien.Dat)
                .Select(x => $"{x.Ten}: {x.GiaiThich}"));
        // Xếp loại tốt nghiệp chỉ ghi khi được công nhận; đủ điều kiện thì chắc chắn đã có CPA
        var xepLoai = ketQua == KetQuaXetTotNghiep.DuDieuKien ? check.XepLoaiTotNghiep : null;

        if (last is not null && last.KetQua == ketQua &&
            last.TongTinChiTichLuy == check.TinChiTichLuy &&
            last.DiemTrungBinhTichLuy == check.Cpa && last.LyDo == lyDo && last.XepLoai == xepLoai)
        {
            await transaction.CommitAsync();
            return await GetRecordAsync(last.Id);
        }

        var result = new XetTotNghiep
        {
            SinhVienId = sinhVienId,
            NgayXet = ThoiGian.HomNay(),
            TongTinChiTichLuy = check.TinChiTichLuy,
            DiemTrungBinhTichLuy = check.Cpa,
            XepLoai = xepLoai,
            KetQua = ketQua,
            LyDo = lyDo
        };
        _db.Set<XetTotNghiep>().Add(result);
        _nhatKy.Ghi($"xét tốt nghiệp {sv.MSSV}: {ketQua}" + (xepLoai is null ? "" : $", xếp loại {xepLoai}"));
        if (ketQua == KetQuaXetTotNghiep.DuDieuKien)
            sv.TrangThai = TrangThaiSinhVien.DaTotNghiep;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetRecordAsync(result.Id);
    }

    public async Task<ChiTietXetTotNghiepDto> GetBySinhVienIdAsync(int sinhVienId)
    {
        if (!await _db.Set<SinhVien>().AsNoTracking().AnyAsync(x => x.Id == sinhVienId))
            throw AppException.NotFound("sinh viên");
        var history = await Project(_db.Set<XetTotNghiep>().AsNoTracking()
            .Where(x => x.SinhVienId == sinhVienId).OrderByDescending(x => x.Id)).ToListAsync();
        if (history.Count == 0)
            throw new AppException(StatusCodes.Status404NotFound,
                "Sinh viên chưa có kết quả xét tốt nghiệp.");
        return new ChiTietXetTotNghiepDto(history[0], history);
    }

    public Task<PagedResult<XetTotNghiepDto>> GetPagedAsync(XetTotNghiepQuery query)
    {
        if (query.KetQua is not null && !Enum.IsDefined(query.KetQua.Value))
            throw AppException.BadRequest("Kết quả xét tốt nghiệp không hợp lệ.");

        var latestIds = _db.Set<XetTotNghiep>().AsNoTracking()
            .GroupBy(x => x.SinhVienId).Select(g => g.Max(x => x.Id));
        var records = _db.Set<XetTotNghiep>().AsNoTracking()
            .Where(x => latestIds.Contains(x.Id));
        if (query.KetQua is KetQuaXetTotNghiep ketQua)
            records = records.Where(x => x.KetQua == ketQua);
        if (query.DaTotNghiep is bool daTotNghiep)
            records = records.Where(x => _db.Set<SinhVien>().Any(s => s.Id == x.SinhVienId &&
                (s.TrangThai == TrangThaiSinhVien.DaTotNghiep) == daTotNghiep));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            records = records.Where(x => _db.Set<SinhVien>().Any(s => s.Id == x.SinhVienId &&
                (s.MSSV.Contains(search) || s.HoTen.Contains(search))));
        }

        return records.OrderByDescending(x => x.NgayXet).ThenByDescending(x => x.Id)
            .ToPagedResultAsync(query, Project);
    }

    private async Task<XetTotNghiepDto> GetRecordAsync(int id) =>
        await Project(_db.Set<XetTotNghiep>().AsNoTracking().Where(x => x.Id == id))
            .FirstAsync();

    private IQueryable<XetTotNghiepDto> Project(IQueryable<XetTotNghiep> records) =>
        from result in records
        join sv in _db.Set<SinhVien>().AsNoTracking() on result.SinhVienId equals sv.Id
        select new XetTotNghiepDto(result.Id, sv.Id, sv.MSSV, sv.HoTen,
            sv.TrangThai.ToString(), result.NgayXet, result.TongTinChiTichLuy,
            result.DiemTrungBinhTichLuy, result.XepLoai,
            result.KetQua.ToString(), result.LyDo);
}
