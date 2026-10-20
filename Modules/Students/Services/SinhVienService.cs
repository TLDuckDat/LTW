using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Services;

public class SinhVienService(AppDbContext db, NhatKyContext nhatKy) : ISinhVienService
{
    private readonly AppDbContext _db = db;
    private readonly NhatKyContext _nhatKy = nhatKy;

    // STU-01 & STU-03: Danh sách sinh viên (phân trang, lọc theo MSSV, Họ tên, Khoa, Ngành, Lớp, Trạng thái)
    public async Task<PagedResult<SinhVienDto>> GetPagedAsync(SinhVienFilterQuery query)
    {
        var q = _db.Set<SinhVien>().AsNoTracking();

        // Lọc theo Khoa (subquery qua Nganh)
        if (query.KhoaId.HasValue)
        {
            var nganhIds = _db.Set<Nganh>().Where(n => n.KhoaId == query.KhoaId.Value).Select(n => n.Id);
            q = q.Where(sv => nganhIds.Contains(sv.NganhId));
        }

        if (query.NganhId.HasValue)
        {
            q = q.Where(sv => sv.NganhId == query.NganhId.Value);
        }

        if (query.LopHanhChinhId.HasValue)
        {
            q = q.Where(sv => sv.LopHanhChinhId == query.LopHanhChinhId.Value);
        }

        if (query.TrangThai.HasValue)
        {
            q = q.Where(sv => sv.TrangThai == query.TrangThai.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(sv => sv.MSSV.Contains(s) || sv.HoTen.Contains(s));
        }

        var total = await q.CountAsync();

        // Join bảng Nganh và LopHanhChinh để lấy tên hiển thị
        var queryJoin = from sv in q
                        join ng in _db.Set<Nganh>().AsNoTracking() on sv.NganhId equals ng.Id into ngGroup
                        from n in ngGroup.DefaultIfEmpty()
                        join lp in _db.Set<LopHanhChinh>().AsNoTracking() on sv.LopHanhChinhId equals lp.Id into lpGroup
                        from l in lpGroup.DefaultIfEmpty()
                        orderby sv.MSSV
                        select new SinhVienDto(
                            sv.Id,
                            sv.MSSV,
                            sv.HoTen,
                            sv.NgaySinh,
                            sv.GioiTinh,
                            sv.Email,
                            sv.SoDienThoai,
                            sv.NganhId,
                            n != null ? n.Ten : null,
                            sv.LopHanhChinhId,
                            l != null ? l.Ten : null,
                            sv.TrangThai,
                            sv.CreatedAt,
                            sv.UpdatedAt
                        );

        var items = await queryJoin
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<SinhVienDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    // STU-02: Xem chi tiết hồ sơ sinh viên
    public async Task<SinhVienDetailDto> GetByIdAsync(int id)
    {
        var sv = await _db.Set<SinhVien>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        var nganh = await _db.Set<Nganh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == sv.NganhId);
        Khoa? khoa = null;
        if (nganh != null)
        {
            khoa = await _db.Set<Khoa>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == nganh.KhoaId);
        }

        ChuyenNganh? chuyenNganh = null;
        if (sv.ChuyenNganhId.HasValue)
        {
            chuyenNganh = await _db.Set<ChuyenNganh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == sv.ChuyenNganhId.Value);
        }

        LopHanhChinh? lop = null;
        GiangVien? coVan = null;
        if (sv.LopHanhChinhId.HasValue)
        {
            lop = await _db.Set<LopHanhChinh>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == sv.LopHanhChinhId.Value);
            if (lop?.CoVanId.HasValue == true)
            {
                coVan = await _db.Set<GiangVien>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == lop.CoVanId.Value);
            }
        }

        return new SinhVienDetailDto(
            sv.Id,
            sv.MSSV,
            sv.HoTen,
            sv.NgaySinh,
            sv.GioiTinh,
            sv.CCCD,
            sv.Email,
            sv.SoDienThoai,
            sv.DiaChi,
            sv.NganhId,
            nganh?.Ma,
            nganh?.Ten,
            khoa?.Id,
            khoa?.Ten,
            sv.ChuyenNganhId,
            chuyenNganh?.Ten,
            sv.LopHanhChinhId,
            lop?.Ma,
            lop?.Ten,
            coVan?.HoTen,
            sv.TrangThai,
            sv.CreatedAt,
            sv.UpdatedAt
        );
    }

    // STU-01: Thêm sinh viên mới
    public async Task<SinhVienDto> CreateAsync(CreateSinhVienRequest request)
    {
        if (await _db.Set<SinhVien>().AnyAsync(x => x.MSSV == request.MSSV))
            throw AppException.Conflict($"Mã sinh viên '{request.MSSV}' đã tồn tại.");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw AppException.Conflict($"Ngành đào tạo với Id {request.NganhId} không tồn tại.");

        if (request.ChuyenNganhId.HasValue && !await _db.Set<ChuyenNganh>().AnyAsync(x => x.Id == request.ChuyenNganhId.Value))
            throw AppException.Conflict($"Chuyên ngành với Id {request.ChuyenNganhId.Value} không tồn tại.");

        if (request.LopHanhChinhId.HasValue && !await _db.Set<LopHanhChinh>().AnyAsync(x => x.Id == request.LopHanhChinhId.Value))
            throw AppException.Conflict($"Lớp hành chính với Id {request.LopHanhChinhId.Value} không tồn tại.");

        var sv = new SinhVien
        {
            MSSV = request.MSSV.Trim(),
            HoTen = request.HoTen.Trim(),
            NgaySinh = request.NgaySinh,
            GioiTinh = request.GioiTinh?.Trim(),
            CCCD = request.CCCD?.Trim(),
            Email = request.Email?.Trim(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            DiaChi = request.DiaChi?.Trim(),
            NganhId = request.NganhId,
            ChuyenNganhId = request.ChuyenNganhId,
            LopHanhChinhId = request.LopHanhChinhId,
            TrangThai = TrangThaiSinhVien.DangHoc
        };

        _db.Add(sv);
        await _db.SaveChangesAsync();

        return await ToDtoAsync(sv);
    }

    // STU-01: Cập nhật thông tin sinh viên
    public async Task<SinhVienDto> UpdateAsync(int id, UpdateSinhVienRequest request)
    {
        var sv = await _db.Set<SinhVien>().FindAsync(id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        if (!await _db.Set<Nganh>().AnyAsync(x => x.Id == request.NganhId))
            throw AppException.Conflict($"Ngành đào tạo với Id {request.NganhId} không tồn tại.");

        if (request.ChuyenNganhId.HasValue && !await _db.Set<ChuyenNganh>().AnyAsync(x => x.Id == request.ChuyenNganhId.Value))
            throw AppException.Conflict($"Chuyên ngành với Id {request.ChuyenNganhId.Value} không tồn tại.");

        if (request.LopHanhChinhId.HasValue && !await _db.Set<LopHanhChinh>().AnyAsync(x => x.Id == request.LopHanhChinhId.Value))
            throw AppException.Conflict($"Lớp hành chính với Id {request.LopHanhChinhId.Value} không tồn tại.");

        sv.HoTen = request.HoTen.Trim();
        sv.NgaySinh = request.NgaySinh;
        sv.GioiTinh = request.GioiTinh?.Trim();
        sv.CCCD = request.CCCD?.Trim();
        sv.Email = request.Email?.Trim();
        sv.SoDienThoai = request.SoDienThoai?.Trim();
        sv.DiaChi = request.DiaChi?.Trim();
        sv.NganhId = request.NganhId;
        sv.ChuyenNganhId = request.ChuyenNganhId;
        sv.LopHanhChinhId = request.LopHanhChinhId;

        await _db.SaveChangesAsync();

        return await ToDtoAsync(sv);
    }

    // STU-01: Xóa sinh viên
    public async Task DeleteAsync(int id)
    {
        var sv = await _db.Set<SinhVien>().FindAsync(id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        _nhatKy.Ghi($"xóa sinh viên {sv.MSSV} - {sv.HoTen}");
        _db.Remove(sv);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw AppException.Conflict("Không thể xóa sinh viên đang có dữ liệu liên quan (học phí, điểm, đăng ký học phần).");
        }
    }

    // STU-04: Đổi lớp hành chính cho 1 sinh viên
    public async Task ChuyenLopAsync(int id, int? lopHanhChinhId)
    {
        var sv = await _db.Set<SinhVien>().FindAsync(id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        if (lopHanhChinhId.HasValue && !await _db.Set<LopHanhChinh>().AnyAsync(x => x.Id == lopHanhChinhId.Value))
            throw AppException.Conflict($"Lớp hành chính với Id {lopHanhChinhId.Value} không tồn tại.");

        sv.LopHanhChinhId = lopHanhChinhId;
        await _db.SaveChangesAsync();
    }

    // STU-06: Đổi trạng thái học tập
    public async Task<SinhVienDto> ChangeStatusAsync(int id, ChangeStatusRequest request)
    {
        var sv = await _db.Set<SinhVien>().FindAsync(id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        // Query/JSON dạng số (ví dụ 9) vẫn bind được vào enum, nên phải kiểm tra lại
        if (!Enum.IsDefined(request.TrangThai))
            throw AppException.BadRequest("Trạng thái học tập không hợp lệ.");
        // Tốt nghiệp chỉ được ghi nhận qua xét tốt nghiệp (module Graduation) để luôn có kết quả xét đi kèm
        if (request.TrangThai == TrangThaiSinhVien.DaTotNghiep)
            throw AppException.Conflict("Trạng thái đã tốt nghiệp chỉ được cập nhật qua xét tốt nghiệp.");
        if (sv.TrangThai == TrangThaiSinhVien.DaTotNghiep)
            throw AppException.Conflict("Sinh viên đã tốt nghiệp, không thể đổi trạng thái học tập.");

        if (sv.TrangThai != request.TrangThai)
        {
            var lichSu = new LichSuHocTap
            {
                SinhVienId = sv.Id,
                TrangThaiCu = sv.TrangThai,
                TrangThaiMoi = request.TrangThai,
                LyDo = request.LyDo ?? "Thay đổi trạng thái học tập",
                SoQuyetDinh = request.SoQuyetDinh,
                NgayHieuLuc = ThoiGian.HomNay()
            };
            _db.Add(lichSu);
            _nhatKy.Ghi($"sinh viên {sv.MSSV}: {sv.TrangThai} -> {request.TrangThai}");
            sv.TrangThai = request.TrangThai;
            await _db.SaveChangesAsync();
        }

        return await ToDtoAsync(sv);
    }

    // STU-07: Tiếp nhận bảo lưu
    public async Task<SinhVienDto> BaoLuuAsync(int id, BaoLuuRequest request)
    {
        var sv = await _db.Set<SinhVien>().FindAsync(id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        if (sv.TrangThai == TrangThaiSinhVien.BaoLuu)
            throw AppException.BadRequest("Sinh viên hiện đang trong trạng thái bảo lưu.");

        if (sv.TrangThai == TrangThaiSinhVien.ThoiHoc || sv.TrangThai == TrangThaiSinhVien.DaTotNghiep)
            throw AppException.BadRequest($"Không thể bảo lưu sinh viên đã {sv.TrangThai}.");

        var lichSu = new LichSuHocTap
        {
            SinhVienId = sv.Id,
            TrangThaiCu = sv.TrangThai,
            TrangThaiMoi = TrangThaiSinhVien.BaoLuu,
            LyDo = request.LyDo,
            SoQuyetDinh = request.SoQuyetDinh,
            NgayHieuLuc = request.NgayHieuLuc ?? ThoiGian.HomNay(),
            GhiChu = request.GhiChu
        };

        _db.Add(lichSu);
        _nhatKy.Ghi($"sinh viên {sv.MSSV}: {sv.TrangThai} -> BaoLuu");
        sv.TrangThai = TrangThaiSinhVien.BaoLuu;
        await _db.SaveChangesAsync();

        return await ToDtoAsync(sv);
    }

    // STU-07: Buộc thôi học / xin thôi học
    public async Task<SinhVienDto> ThoiHocAsync(int id, ThoiHocRequest request)
    {
        var sv = await _db.Set<SinhVien>().FindAsync(id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        if (sv.TrangThai == TrangThaiSinhVien.ThoiHoc)
            throw AppException.BadRequest("Sinh viên hiện đã có quyết định thôi học.");
        if (sv.TrangThai == TrangThaiSinhVien.DaTotNghiep)
            throw AppException.BadRequest("Không thể thôi học đối với sinh viên đã tốt nghiệp.");

        var lichSu = new LichSuHocTap
        {
            SinhVienId = sv.Id,
            TrangThaiCu = sv.TrangThai,
            TrangThaiMoi = TrangThaiSinhVien.ThoiHoc,
            LyDo = request.LyDo,
            SoQuyetDinh = request.SoQuyetDinh,
            NgayHieuLuc = request.NgayHieuLuc ?? ThoiGian.HomNay(),
            GhiChu = request.GhiChu
        };

        _db.Add(lichSu);
        _nhatKy.Ghi($"sinh viên {sv.MSSV}: {sv.TrangThai} -> ThoiHoc");
        sv.TrangThai = TrangThaiSinhVien.ThoiHoc;
        await _db.SaveChangesAsync();

        return await ToDtoAsync(sv);
    }

    // STU-07: Quay lại học tập
    public async Task<SinhVienDto> QuayLaiHocAsync(int id, string? ghiChu)
    {
        var sv = await _db.Set<SinhVien>().FindAsync(id);
        if (sv is null) throw AppException.NotFound("sinh viên");

        if (sv.TrangThai == TrangThaiSinhVien.DangHoc)
            throw AppException.BadRequest("Sinh viên hiện đang học bình thường.");
        if (sv.TrangThai == TrangThaiSinhVien.DaTotNghiep)
            throw AppException.BadRequest("Sinh viên đã tốt nghiệp, không thể quay lại học.");

        var lichSu = new LichSuHocTap
        {
            SinhVienId = sv.Id,
            TrangThaiCu = sv.TrangThai,
            TrangThaiMoi = TrangThaiSinhVien.DangHoc,
            LyDo = "Hết thời gian bảo lưu / khôi phục học tập",
            NgayHieuLuc = ThoiGian.HomNay(),
            GhiChu = ghiChu
        };

        _db.Add(lichSu);
        _nhatKy.Ghi($"sinh viên {sv.MSSV}: {sv.TrangThai} -> DangHoc");
        sv.TrangThai = TrangThaiSinhVien.DangHoc;
        await _db.SaveChangesAsync();

        return await ToDtoAsync(sv);
    }

    // STU-07: Xem lịch sử học tập
    public async Task<List<LichSuHocTapDto>> GetLichSuHocTapAsync(int id)
    {
        return await _db.Set<LichSuHocTap>()
            .AsNoTracking()
            .Where(x => x.SinhVienId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new LichSuHocTapDto(
                x.Id,
                x.SinhVienId,
                x.TrangThaiCu,
                x.TrangThaiMoi,
                x.SoQuyetDinh,
                x.NgayHieuLuc,
                x.LyDo,
                x.GhiChu,
                x.CreatedAt
            ))
            .ToListAsync();
    }

    // Kèm tên ngành và tên lớp hành chính để hiển thị
    private async Task<SinhVienDto> ToDtoAsync(SinhVien sv)
    {
        var nganh = await _db.Set<Nganh>().FindAsync(sv.NganhId);
        var lop = sv.LopHanhChinhId.HasValue ? await _db.Set<LopHanhChinh>().FindAsync(sv.LopHanhChinhId.Value) : null;

        return new SinhVienDto(
            sv.Id, sv.MSSV, sv.HoTen, sv.NgaySinh, sv.GioiTinh, sv.Email, sv.SoDienThoai,
            sv.NganhId, nganh?.Ten, sv.LopHanhChinhId, lop?.Ten, sv.TrangThai, sv.CreatedAt, sv.UpdatedAt
        );
    }
}
