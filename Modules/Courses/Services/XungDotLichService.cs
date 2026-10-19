using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Services;

public class XungDotLichService(AppDbContext db) : IXungDotLichService
{
    private readonly AppDbContext _db = db;

    public async Task<IReadOnlyList<XungDotDto>> TimXungDotAsync(
        LopHocPhan lop, int? giangVienId, int? phongHocId, int thu, int tietBatDau, int soTiet, int? boQuaLichHocId = null)
    {
        var tietKetThuc = tietBatDau + soTiet - 1;

        // Hai khoảng tiết [a1,a2] và [b1,b2] giao nhau khi a1 <= b2 và b1 <= a2
        var coThe = await (
            from l in _db.Set<LichHoc>().AsNoTracking()
            join lhp in _db.Set<LopHocPhan>().AsNoTracking() on l.LopHocPhanId equals lhp.Id
            where lhp.HocKyId == lop.HocKyId
                  && lhp.TrangThai != TrangThaiLopHocPhan.DaHuy
                  && l.Thu == thu
                  && l.TietBatDau <= tietKetThuc
                  && tietBatDau <= l.TietBatDau + l.SoTiet - 1
                  && (boQuaLichHocId == null || l.Id != boQuaLichHocId)
            select new { Lich = l, MaLop = lhp.Ma, lhp.GiangVienId })
            .ToListAsync();

        var ketQua = new List<XungDotDto>();
        foreach (var x in coThe)
        {
            var l = x.Lich;
            var gio = $"thứ {l.Thu}, tiết {l.TietBatDau}-{l.TietBatDau + l.SoTiet - 1}";

            if (l.LopHocPhanId == lop.Id)
            {
                ketQua.Add(new(LoaiXungDot.Lop, l.Id, l.LopHocPhanId, x.MaLop, l.Thu, l.TietBatDau, l.SoTiet,
                    $"Trùng với buổi học khác của chính lớp {x.MaLop} ({gio})."));
                continue;
            }

            if (phongHocId is not null && l.PhongHocId == phongHocId)
                ketQua.Add(new(LoaiXungDot.Phong, l.Id, l.LopHocPhanId, x.MaLop, l.Thu, l.TietBatDau, l.SoTiet,
                    $"Phòng học đã được lớp {x.MaLop} sử dụng ({gio})."));

            if (giangVienId is not null && x.GiangVienId == giangVienId)
                ketQua.Add(new(LoaiXungDot.GiangVien, l.Id, l.LopHocPhanId, x.MaLop, l.Thu, l.TietBatDau, l.SoTiet,
                    $"Giảng viên đã có lớp {x.MaLop} ({gio})."));
        }
        return ketQua;
    }

    public async Task<IReadOnlyList<XungDotDto>> TimXungDotGiangVienAsync(LopHocPhan lop, int giangVienId)
    {
        var cacBuoi = await _db.Set<LichHoc>().AsNoTracking().Where(x => x.LopHocPhanId == lop.Id).ToListAsync();

        var ketQua = new List<XungDotDto>();
        foreach (var buoi in cacBuoi)
        {
            var xungDot = await TimXungDotAsync(lop, giangVienId, null, buoi.Thu, buoi.TietBatDau, buoi.SoTiet, buoi.Id);
            ketQua.AddRange(xungDot.Where(x => x.Loai == LoaiXungDot.GiangVien));
        }
        return ketQua;
    }
}
