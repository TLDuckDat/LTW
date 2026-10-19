using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Grades.Services;

public class DiemService(AppDbContext db) : IDiemService
{
    private readonly AppDbContext _db = db;

    public async Task<NhapDiemResultDto> NhapDiemAsync(int lopHocPhanId, NhapDiemRequest request)
    {
        var lop = await _db.GetLopHocPhanAsync(lopHocPhanId);
        KiemTraLopDuocNhapDiem(lop);

        // DataAnnotations bỏ qua phần tử null trong danh sách, nên phải kiểm tra ở đây
        if (request.DanhSachDiem.Any(x => x is null))
            throw AppException.BadRequest("Danh sách điểm có phần tử rỗng.");

        var dangKyIds = request.DanhSachDiem.Select(x => x.DangKyHocPhanId).ToList();
        if (dangKyIds.Distinct().Count() != dangKyIds.Count)
            throw AppException.BadRequest("Danh sách có mã đăng ký trùng nhau.");

        var dangKyTrongLop = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.LopHocPhanId == lopHocPhanId && dangKyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);
        var diemHienCo = await _db.Set<DiemHocPhan>()
            .Where(x => dangKyIds.Contains(x.DangKyHocPhanId))
            .ToDictionaryAsync(x => x.DangKyHocPhanId);

        // Bước 1: kiểm tra toàn bộ danh sách trước, chỉ lưu khi mọi phần tử đều hợp lệ
        foreach (var item in request.DanhSachDiem)
        {
            var id = item.DangKyHocPhanId;
            if (!dangKyTrongLop.TryGetValue(id, out var dangKy))
                throw AppException.BadRequest($"Lượt đăng ký {id} không thuộc lớp học phần này.");
            if (dangKy.TrangThai != TrangThaiDangKy.DaDangKy)
                throw AppException.BadRequest($"Lượt đăng ký {id} đã bị hủy.");
            if (item.DiemQuaTrinh is null && item.DiemGiuaKy is null && item.DiemCuoiKy is null)
                throw AppException.BadRequest($"Lượt đăng ký {id} phải có ít nhất một điểm thành phần.");
            if (CoHonHaiChuSoThapPhan(item))
                throw AppException.BadRequest($"Điểm của lượt đăng ký {id} chỉ được có tối đa 2 chữ số thập phân.");
            if (diemHienCo.TryGetValue(id, out var diemCu) && diemCu.DaKhoa)
                throw AppException.Conflict($"Điểm của lượt đăng ký {id} đã khóa, không thể cập nhật.");
        }

        // Bước 2: thêm mới hoặc cập nhật
        var soThemMoi = 0;
        var soCapNhat = 0;
        var soKhongThayDoi = 0;
        foreach (var item in request.DanhSachDiem)
        {
            if (!diemHienCo.TryGetValue(item.DangKyHocPhanId, out var diem))
            {
                _db.Add(new DiemHocPhan
                {
                    DangKyHocPhanId = item.DangKyHocPhanId,
                    DiemQuaTrinh = item.DiemQuaTrinh,
                    DiemGiuaKy = item.DiemGiuaKy,
                    DiemCuoiKy = item.DiemCuoiKy
                });
                soThemMoi++;
            }
            else if (diem.DiemQuaTrinh == item.DiemQuaTrinh && diem.DiemGiuaKy == item.DiemGiuaKy && diem.DiemCuoiKy == item.DiemCuoiKy)
            {
                soKhongThayDoi++;
            }
            else
            {
                // Đổi điểm thành phần thì điểm tổng kết, điểm chữ cũ không còn đúng: xóa để GRADE-03 tính lại
                diem.DiemQuaTrinh = item.DiemQuaTrinh;
                diem.DiemGiuaKy = item.DiemGiuaKy;
                diem.DiemCuoiKy = item.DiemCuoiKy;
                diem.DiemTongKet = null;
                diem.DiemChu = null;
                soCapNhat++;
            }
        }

        await _db.SaveChangesAsync();
        return new NhapDiemResultDto(lopHocPhanId, request.DanhSachDiem.Count, soThemMoi, soCapNhat, soKhongThayDoi);
    }

    public async Task<TinhDiemResultDto> TinhDiemAsync(int lopHocPhanId)
    {
        var lop = await _db.GetLopHocPhanAsync(lopHocPhanId);
        if (lop.TrangThai is not (TrangThaiLopHocPhan.DangHoc or TrangThaiLopHocPhan.DaKetThuc))
            throw AppException.Conflict("Lớp học phần chưa bắt đầu hoặc đã bị hủy, không thể tính điểm.");

        var danhSachDiem = await _db.Set<DiemHocPhan>()
            .Join(_db.Set<DangKyHocPhan>()
                    .Where(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy),
                diem => diem.DangKyHocPhanId,
                dangKy => dangKy.Id,
                (diem, dangKy) => diem)
            .ToListAsync();

        var soDaTinh = 0;
        var soThieuDiem = 0;
        var soDaKhoa = 0;
        var soKhongThayDoi = 0;
        foreach (var diem in danhSachDiem)
        {
            if (diem.DaKhoa)
            {
                soDaKhoa++;
                continue;
            }
            if (!GradeCalculator.DuDiemThanhPhan(diem))
            {
                soThieuDiem++;
                continue;
            }

            var tongKet = GradeCalculator.TinhDiemTongKet(
                diem.DiemQuaTrinh!.Value, diem.DiemGiuaKy!.Value, diem.DiemCuoiKy!.Value);
            var diemChu = GradeCalculator.QuyDoiDiemChu(tongKet);
            if (diem.DiemTongKet == tongKet && diem.DiemChu == diemChu)
            {
                soKhongThayDoi++;
                continue;
            }

            diem.DiemTongKet = tongKet;
            diem.DiemChu = diemChu;
            soDaTinh++;
        }

        if (soDaTinh > 0) await _db.SaveChangesAsync();
        return new TinhDiemResultDto(
            lopHocPhanId, danhSachDiem.Count, soDaTinh, soThieuDiem, soDaKhoa, soKhongThayDoi);
    }

    public async Task<KhoaDiemResultDto> KhoaDiemAsync(int lopHocPhanId)
    {
        var lop = await _db.GetLopHocPhanAsync(lopHocPhanId);
        if (lop.TrangThai != TrangThaiLopHocPhan.DaKetThuc)
            throw AppException.Conflict("Chỉ được duyệt và khóa bảng điểm khi lớp học phần đã kết thúc.");

        var dangKyIds = await _db.Set<DangKyHocPhan>().AsNoTracking()
            .Where(x => x.LopHocPhanId == lopHocPhanId && x.TrangThai == TrangThaiDangKy.DaDangKy)
            .Select(x => x.Id)
            .ToListAsync();
        if (dangKyIds.Count == 0)
            throw AppException.Conflict("Lớp học phần không có sinh viên đang đăng ký để duyệt bảng điểm.");

        var diemTheoDangKy = await _db.Set<DiemHocPhan>()
            .Where(x => dangKyIds.Contains(x.DangKyHocPhanId))
            .ToDictionaryAsync(x => x.DangKyHocPhanId);

        // Đếm từ danh sách đăng ký (không phải danh sách điểm) để không bỏ sót sinh viên chưa có bản ghi điểm
        var soThieuDiem = dangKyIds.Count(id =>
            !diemTheoDangKy.TryGetValue(id, out var diem) ||
            !GradeCalculator.DuDiemThanhPhan(diem) || !GradeCalculator.DaTinhTongKet(diem));
        if (soThieuDiem > 0)
            throw AppException.Conflict($"Có {soThieuDiem} sinh viên chưa đủ điểm để duyệt và khóa bảng điểm.");

        var soDaKhoaTruocDo = 0;
        foreach (var diem in diemTheoDangKy.Values)
        {
            if (diem.DaKhoa) soDaKhoaTruocDo++;
            else diem.DaKhoa = true;
        }

        var soMoiKhoa = dangKyIds.Count - soDaKhoaTruocDo;
        if (soMoiKhoa > 0) await _db.SaveChangesAsync();
        return new KhoaDiemResultDto(lopHocPhanId, dangKyIds.Count, soMoiKhoa, soDaKhoaTruocDo);
    }

    // Chỉ nhập điểm khi lớp đã bắt đầu học hoặc đã kết thúc
    private static void KiemTraLopDuocNhapDiem(LopHocPhan lop)
    {
        if (lop.TrangThai is TrangThaiLopHocPhan.DangHoc or TrangThaiLopHocPhan.DaKetThuc) return;

        var lyDo = lop.TrangThai == TrangThaiLopHocPhan.DaHuy ? "đã bị hủy" : "còn đang mở đăng ký";
        throw AppException.Conflict($"Lớp học phần {lyDo}, chưa thể nhập điểm.");
    }

    private static bool CoHonHaiChuSoThapPhan(DiemSinhVienRequest item) =>
        new[] { item.DiemQuaTrinh, item.DiemGiuaKy, item.DiemCuoiKy }
            .Any(diem => diem is not null && !GradeCalculator.CoToiDaHaiChuSoThapPhan(diem.Value));
}
