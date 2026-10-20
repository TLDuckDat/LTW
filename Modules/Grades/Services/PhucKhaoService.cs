using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Grades.Services;

public class PhucKhaoService(AppDbContext db, NhatKyContext nhatKy) : IPhucKhaoService
{
    private readonly AppDbContext _db = db;
    private readonly NhatKyContext _nhatKy = nhatKy;

    public async Task<PhucKhaoDto> CreateAsync(PhucKhaoRequest request, int? chiSinhVienId = null)
    {
        var diemQuery = _db.Set<DiemHocPhan>().AsNoTracking().Where(x => x.Id == request.DiemHocPhanId);
        if (chiSinhVienId is { } sinhVienId)
        {
            var diemCuaSinhVien = DiemCuaSinhVien(sinhVienId);
            diemQuery = diemQuery.Where(x => diemCuaSinhVien.Contains(x.Id));
        }
        var diem = await diemQuery.FirstOrDefaultAsync()
            ?? throw AppException.NotFound("bản ghi điểm");

        if (!await ThuocLopDaKetThucAsync(diem))
            throw AppException.Conflict("Chỉ phúc khảo điểm của lượt đăng ký còn hiệu lực trong lớp đã kết thúc.");
        if (!diem.DaKhoa || diem.DiemCuoiKy is null)
            throw AppException.Conflict("Điểm cuối kỳ phải có giá trị và đã được duyệt, khóa trước khi phúc khảo.");
        if (await _db.Set<PhucKhao>().AnyAsync(x => x.DiemHocPhanId == diem.Id && x.TrangThai == TrangThaiPhucKhao.ChoXuLy))
            throw AppException.Conflict("Bản ghi điểm đã có yêu cầu phúc khảo đang chờ xử lý.");

        var phucKhao = new PhucKhao
        {
            DiemHocPhanId = diem.Id,
            LyDo = request.LyDo.Trim(),
            DiemCu = diem.DiemCuoiKy,
            TrangThai = TrangThaiPhucKhao.ChoXuLy
        };
        _db.Add(phucKhao);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(phucKhao.Id);
    }

    public async Task<PagedResult<PhucKhaoDto>> GetPagedAsync(
        PagedQuery query, TrangThaiPhucKhao? trangThai, int? chiSinhVienId = null)
    {
        var phucKhao = CuaSinhVien(_db.Set<PhucKhao>().AsNoTracking(), chiSinhVienId);
        if (trangThai is not null)
        {
            // Query string dạng số (ví dụ ?trangThai=9) vẫn bind được vào enum, nên phải kiểm tra lại
            if (!Enum.IsDefined(trangThai.Value))
                throw AppException.BadRequest("Trạng thái phúc khảo không hợp lệ.");
            phucKhao = phucKhao.Where(x => x.TrangThai == trangThai);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            var diemCuaSinhVienPhuHop =
                from diem in _db.Set<DiemHocPhan>()
                join dangKy in _db.Set<DangKyHocPhan>() on diem.DangKyHocPhanId equals dangKy.Id
                join sinhVien in _db.Set<SinhVien>() on dangKy.SinhVienId equals sinhVien.Id
                where sinhVien.MSSV.Contains(search) || sinhVien.HoTen.Contains(search)
                select diem.Id;
            phucKhao = phucKhao.Where(x => diemCuaSinhVienPhuHop.Contains(x.DiemHocPhanId));
        }

        return await phucKhao.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .ToPagedResultAsync(query, Project);
    }

    public async Task<PhucKhaoDto> GetByIdAsync(int id, int? chiSinhVienId = null) =>
        await Project(CuaSinhVien(_db.Set<PhucKhao>().AsNoTracking(), chiSinhVienId).Where(x => x.Id == id))
            .FirstOrDefaultAsync()
            ?? throw AppException.NotFound("yêu cầu phúc khảo");

    /// <summary>Id các bản ghi điểm thuộc lượt đăng ký của sinh viên.</summary>
    private IQueryable<int> DiemCuaSinhVien(int sinhVienId) =>
        from diem in _db.Set<DiemHocPhan>()
        join dangKy in _db.Set<DangKyHocPhan>() on diem.DangKyHocPhanId equals dangKy.Id
        where dangKy.SinhVienId == sinhVienId
        select diem.Id;

    // Phúc khảo của sinh viên khác coi như không tồn tại (404), không tiết lộ là có hay không
    private IQueryable<PhucKhao> CuaSinhVien(IQueryable<PhucKhao> phucKhao, int? chiSinhVienId)
    {
        if (chiSinhVienId is not { } id) return phucKhao;
        var diemCuaSinhVien = DiemCuaSinhVien(id);
        return phucKhao.Where(x => diemCuaSinhVien.Contains(x.DiemHocPhanId));
    }

    public async Task<PhucKhaoDto> XuLyAsync(int id, XuLyPhucKhaoRequest request)
    {
        var phucKhao = await _db.Set<PhucKhao>().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound("yêu cầu phúc khảo");
        if (phucKhao.TrangThai != TrangThaiPhucKhao.ChoXuLy)
            throw AppException.Conflict("Yêu cầu phúc khảo đã được xử lý.");

        var diem = await _db.Set<DiemHocPhan>().FirstOrDefaultAsync(x => x.Id == phucKhao.DiemHocPhanId);
        if (diem is null || !diem.DaKhoa)
            throw AppException.Conflict("Bản ghi điểm không tồn tại hoặc không còn khóa.");
        if (!await ThuocLopDaKetThucAsync(diem))
            throw AppException.Conflict("Lượt đăng ký hoặc lớp học phần không còn hợp lệ để xử lý phúc khảo.");

        if (request.ChapNhan == true)
        {
            var diemCu = diem.DiemCuoiKy;
            ChapNhan(phucKhao, diem, request.DiemMoi);
            _nhatKy.Ghi($"chấp nhận phúc khảo, điểm cuối kỳ {diemCu} -> {diem.DiemCuoiKy}, tổng kết {diem.DiemTongKet} ({diem.DiemChu})");
        }
        else
        {
            TuChoi(phucKhao, request.DiemMoi);
            _nhatKy.Ghi("từ chối phúc khảo");
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw AppException.Conflict("Yêu cầu phúc khảo vừa được người khác xử lý, hãy tải lại.");
        }
        return await GetByIdAsync(phucKhao.Id);
    }

    /// <summary>Chấp nhận: thay điểm cuối kỳ bằng điểm mới rồi tính lại điểm tổng kết và điểm chữ.</summary>
    private static void ChapNhan(PhucKhao phucKhao, DiemHocPhan diem, decimal? diemMoi)
    {
        if (diemMoi is null)
            throw AppException.BadRequest("Cần nhập điểm mới khi chấp nhận phúc khảo.");
        if (!GradeCalculator.CoToiDaHaiChuSoThapPhan(diemMoi.Value))
            throw AppException.BadRequest("Điểm mới chỉ được có tối đa 2 chữ số thập phân.");
        // Điểm cuối kỳ phải còn đúng như lúc tạo yêu cầu, tránh ghi đè một thay đổi khác
        if (phucKhao.DiemCu != diem.DiemCuoiKy || !GradeCalculator.DuDiemThanhPhan(diem) || !GradeCalculator.DaTinhTongKet(diem))
            throw AppException.Conflict("Dữ liệu điểm đã khóa không còn đầy đủ hoặc không khớp điểm cũ của yêu cầu.");
        if (diemMoi == diem.DiemCuoiKy)
            throw AppException.Conflict("Điểm mới phải khác điểm cuối kỳ hiện tại.");

        diem.DiemCuoiKy = diemMoi;
        diem.DiemTongKet = GradeCalculator.TinhDiemTongKet(diem.DiemQuaTrinh!.Value, diem.DiemGiuaKy!.Value, diemMoi.Value);
        diem.DiemChu = GradeCalculator.QuyDoiDiemChu(diem.DiemTongKet.Value);
        phucKhao.DiemMoi = diemMoi;
        phucKhao.TrangThai = TrangThaiPhucKhao.DaChapNhan;
    }

    private static void TuChoi(PhucKhao phucKhao, decimal? diemMoi)
    {
        if (diemMoi is not null)
            throw AppException.BadRequest("Không nhập điểm mới khi từ chối phúc khảo.");
        phucKhao.DiemMoi = null;
        phucKhao.TrangThai = TrangThaiPhucKhao.TuChoi;
    }

    /// <summary>Chỉ phúc khảo được điểm của lượt đăng ký còn hiệu lực, trong lớp học phần đã kết thúc.</summary>
    private Task<bool> ThuocLopDaKetThucAsync(DiemHocPhan diem) =>
        (from dangKy in _db.Set<DangKyHocPhan>()
         join lop in _db.Set<LopHocPhan>() on dangKy.LopHocPhanId equals lop.Id
         where dangKy.Id == diem.DangKyHocPhanId
               && dangKy.TrangThai == TrangThaiDangKy.DaDangKy
               && lop.TrangThai == TrangThaiLopHocPhan.DaKetThuc
         select dangKy.Id).AnyAsync();

    private IQueryable<PhucKhaoDto> Project(IQueryable<PhucKhao> phucKhao) =>
        from pk in phucKhao
        join diem in _db.Set<DiemHocPhan>().AsNoTracking() on pk.DiemHocPhanId equals diem.Id
        join dangKy in _db.Set<DangKyHocPhan>().AsNoTracking() on diem.DangKyHocPhanId equals dangKy.Id
        join sinhVien in _db.Set<SinhVien>().AsNoTracking() on dangKy.SinhVienId equals sinhVien.Id
        select new PhucKhaoDto(
            pk.Id, pk.DiemHocPhanId, sinhVien.Id, sinhVien.MSSV, sinhVien.HoTen,
            pk.LyDo, pk.DiemCu, pk.DiemMoi, pk.TrangThai.ToString(), pk.CreatedAt);
}
