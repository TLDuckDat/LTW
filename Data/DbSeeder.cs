using StudentManagement.Common;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Academic.Services;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Grades.Services;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Reports.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Data;

/// <summary>
/// Seed dữ liệu mẫu khi chạy ở môi trường Development (chỉ chạy khi CSDL còn trống).
/// Ngày tháng tính theo ngày chạy để học kỳ hiện tại và đợt đăng ký luôn đang diễn ra:
/// - Năm học trước, học kỳ 2: lớp IT001 đã kết thúc, điểm đã khóa (SV1 đạt, SV2 trượt).
/// - Học kỳ hiện tại: đợt đăng ký đang mở; lớp IT002 (tiên quyết IT001), IT001 (học lại), IT003.
/// Tài khoản mẫu (xem README): admin, pdt, gv001, sv001, sv002.
/// </summary>
public static class DbSeeder
{
    public const string MatKhauAdmin = "Admin@123";
    public const string MatKhauPhongDaoTao = "Pdt@1234";
    public const string MatKhauGiangVien = "Gv@12345";
    public const string MatKhauSinhVien = "Sv@12345";

    public static async Task SeedAsync(AppDbContext db)
    {
        var adminUser = await db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.TenDangNhap == "admin");
        if (adminUser != null && adminUser.MatKhauHash == "CHANGE_ME")
        {
            adminUser.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(MatKhauAdmin);
            await db.SaveChangesAsync();
        }

        if (await db.Set<Khoa>().AnyAsync()) return;

        await using var transaction = await db.Database.BeginTransactionAsync();

        if (adminUser == null)
        {
            adminUser = new TaiKhoan
            {
                TenDangNhap = "admin", MatKhauHash = BCrypt.Net.BCrypt.HashPassword(MatKhauAdmin),
                HoTen = "Quản trị viên", VaiTro = VaiTro.Admin
            };
            db.Add(adminUser);
        }

        // ===== Academic =====
        var cntt = new Khoa { Ma = "CNTT", Ten = "Khoa Công nghệ thông tin" };
        var kt = new Khoa { Ma = "KT", Ten = "Khoa Kinh tế" };
        db.AddRange(cntt, kt);
        await db.SaveChangesAsync();

        var boMon = new BoMon { Ma = "KTPM", Ten = "Bộ môn Kỹ thuật phần mềm", KhoaId = cntt.Id };
        var nganh = new Nganh { Ma = "7480201", Ten = "Công nghệ thông tin", KhoaId = cntt.Id };
        db.AddRange(boMon, nganh);

        // Năm học bắt đầu 15/8; "hôm nay" theo giờ Việt Nam
        var homNay = ThoiGian.HomNay();
        var namBatDau = homNay >= new DateOnly(homNay.Year, 8, 15) ? homNay.Year : homNay.Year - 1;
        var namTruoc = TaoNamHoc(namBatDau - 1);
        var namNay = TaoNamHoc(namBatDau);
        db.AddRange(namTruoc, namNay);
        await db.SaveChangesAsync();

        var hocKyTruoc = TaoHocKy(namTruoc, 2);
        var hocKyNay = new[] { 1, 2, 3 }.Select(i => TaoHocKy(namNay, i))
            .First(hk => hk.NgayBatDau <= homNay && homNay <= hk.NgayKetThuc);
        db.AddRange(hocKyTruoc, hocKyNay);
        foreach (var hk in new[] { 1, 2, 3 }.Select(i => TaoHocKy(namNay, i)).Where(hk => hk.SoThuTu != hocKyNay.SoThuTu))
            db.Add(hk);

        // ===== Courses =====
        var it001 = new MonHoc { Ma = "IT001", Ten = "Nhập môn lập trình", SoTinChi = 3 };
        var it002 = new MonHoc { Ma = "IT002", Ten = "Lập trình hướng đối tượng", SoTinChi = 3 };
        var it003 = new MonHoc { Ma = "IT003", Ten = "Cơ sở dữ liệu", SoTinChi = 3 };
        var a101 = new PhongHoc { Ma = "A101", SucChua = 60 };
        var a102 = new PhongHoc { Ma = "A102", SucChua = 40 };
        db.AddRange(it001, it002, it003, a101, a102);
        await db.SaveChangesAsync();
        db.Add(new MonHocTienQuyet { MonHocId = it002.Id, TienQuyetId = it001.Id });

        // ===== Students =====
        var gv = new GiangVien { Ma = "GV001", HoTen = "Nguyễn Văn An", HocVi = "Thạc sĩ", Email = "an.nv@example.com", BoMonId = boMon.Id };
        db.Add(gv);
        await db.SaveChangesAsync();

        var lop = new LopHanhChinh
        {
            Ma = $"CNTT{namTruoc.NgayBatDau.Year}A", Ten = $"CNTT K{namTruoc.NgayBatDau.Year} A",
            NganhId = nganh.Id, KhoaTuyen = namTruoc.NgayBatDau.Year, CoVanId = gv.Id
        };
        db.Add(lop);
        await db.SaveChangesAsync();

        var sv1 = new SinhVien
        {
            MSSV = $"{lop.KhoaTuyen}000001", HoTen = "Trần Thị Bình", NgaySinh = new DateOnly(lop.KhoaTuyen - 18, 3, 12),
            GioiTinh = "Nữ", Email = "binh@example.com", NganhId = nganh.Id, LopHanhChinhId = lop.Id
        };
        var sv2 = new SinhVien
        {
            MSSV = $"{lop.KhoaTuyen}000002", HoTen = "Lê Văn Cường", NgaySinh = new DateOnly(lop.KhoaTuyen - 18, 7, 5),
            GioiTinh = "Nam", Email = "cuong@example.com", NganhId = nganh.Id, LopHanhChinhId = lop.Id
        };
        db.AddRange(sv1, sv2);

        // Chương trình đào tạo của khóa, đủ điều kiện để xét tốt nghiệp
        var ctdt = new ChuongTrinhDaoTao
        {
            Ma = $"CTDT-CNTT-{lop.KhoaTuyen}", Ten = $"Công nghệ thông tin khóa {lop.KhoaTuyen}", NganhId = nganh.Id,
            KhoaTuyen = lop.KhoaTuyen, TongSoTinChi = 9, CpaToiThieu = 2.0m, YeuCauThucTap = false, YeuCauKhoaLuan = false
        };
        db.Add(ctdt);
        await db.SaveChangesAsync();
        db.AddRange(
            new ChuongTrinhDaoTaoMonHoc { ChuongTrinhDaoTaoId = ctdt.Id, MonHocId = it001.Id, HocKyGoiY = 1 },
            new ChuongTrinhDaoTaoMonHoc { ChuongTrinhDaoTaoId = ctdt.Id, MonHocId = it002.Id, HocKyGoiY = 2 },
            new ChuongTrinhDaoTaoMonHoc { ChuongTrinhDaoTaoId = ctdt.Id, MonHocId = it003.Id, HocKyGoiY = 2 });

        // ===== Học kỳ trước: IT001 đã kết thúc, điểm đã khóa =====
        var lopCu = new LopHocPhan
        {
            Ma = "IT001-01-" + hocKyTruoc.NgayBatDau.Year, HocKyId = hocKyTruoc.Id, MonHocId = it001.Id,
            GiangVienId = gv.Id, SiSoToiDa = 60, TrangThai = TrangThaiLopHocPhan.DaKetThuc
        };
        db.Add(lopCu);
        await db.SaveChangesAsync();
        db.Add(new LichHoc { LopHocPhanId = lopCu.Id, Thu = 2, TietBatDau = 1, SoTiet = 3, PhongHocId = a101.Id });
        var dkCu1 = new DangKyHocPhan { SinhVienId = sv1.Id, LopHocPhanId = lopCu.Id };
        var dkCu2 = new DangKyHocPhan { SinhVienId = sv2.Id, LopHocPhanId = lopCu.Id };
        db.AddRange(dkCu1, dkCu2);
        await db.SaveChangesAsync();
        db.AddRange(DiemDaKhoa(dkCu1.Id, 8.5m, 8m, 9m), DiemDaKhoa(dkCu2.Id, 3m, 2m, 3m));

        // ===== Học kỳ hiện tại: đợt đăng ký đang mở =====
        db.Add(new DotDangKy
        {
            Ten = $"Đăng ký {hocKyNay.Ten} {namNay.Ten}", HocKyId = hocKyNay.Id,
            NgayBatDau = DateTime.UtcNow.AddDays(-1), NgayKetThuc = DateTime.UtcNow.AddDays(14),
            TrangThai = TrangThaiDotDangKy.DangMo
        });
        var lopIt002 = new LopHocPhan { Ma = "IT002-01", HocKyId = hocKyNay.Id, MonHocId = it002.Id, GiangVienId = gv.Id, SiSoToiDa = 60 };
        var lopIt001 = new LopHocPhan { Ma = "IT001-02", HocKyId = hocKyNay.Id, MonHocId = it001.Id, SiSoToiDa = 40 };
        var lopIt003 = new LopHocPhan { Ma = "IT003-01", HocKyId = hocKyNay.Id, MonHocId = it003.Id, SiSoToiDa = 40 };
        db.AddRange(lopIt002, lopIt001, lopIt003);
        await db.SaveChangesAsync();
        db.AddRange(
            new LichHoc { LopHocPhanId = lopIt002.Id, Thu = 2, TietBatDau = 1, SoTiet = 3, PhongHocId = a101.Id },
            new LichHoc { LopHocPhanId = lopIt001.Id, Thu = 3, TietBatDau = 4, SoTiet = 3, PhongHocId = a102.Id },
            new LichHoc { LopHocPhanId = lopIt003.Id, Thu = 4, TietBatDau = 1, SoTiet = 3, PhongHocId = a102.Id });

        // ===== Tài khoản mẫu cho từng vai trò =====
        db.AddRange(
            TaiKhoan("pdt", MatKhauPhongDaoTao, "Phòng Đào tạo", VaiTro.PhongDaoTao),
            TaiKhoan("gv001", MatKhauGiangVien, gv.HoTen, VaiTro.GiangVien, giangVienId: gv.Id),
            TaiKhoan("sv001", MatKhauSinhVien, sv1.HoTen, VaiTro.SinhVien, sinhVienId: sv1.Id),
            TaiKhoan("sv002", MatKhauSinhVien, sv2.HoTen, VaiTro.SinhVien, sinhVienId: sv2.Id));

        // ===== Quy chế: ghi sẵn giá trị mặc định để dễ chỉnh =====
        db.AddRange(
            QuyChe(MaQuyChe.TinChiToiDaHocKy, "Tín chỉ tối đa mỗi học kỳ", "24", "tín chỉ"),
            QuyChe(MaQuyChe.CanhBaoGpaHocKyDauTien, "Cảnh báo: GPA tối thiểu học kỳ đầu", "0.8", "điểm hệ 4"),
            QuyChe(MaQuyChe.CanhBaoGpaToiThieu, "Cảnh báo: GPA tối thiểu", "1.0", "điểm hệ 4"),
            QuyChe(MaQuyChe.CanhBaoTinChiToiThieu, "Cảnh báo: tín chỉ tối thiểu mỗi học kỳ", "12", "tín chỉ"),
            QuyChe(MaQuyChe.DonGiaTinChi, "Đơn giá tín chỉ", "600000", "VND"),
            QuyChe(MaQuyChe.HocBongDiemRenLuyenToiThieu, "Học bổng: điểm rèn luyện tối thiểu", "80", "điểm"),
            QuyChe(MaQuyChe.HocBongGpaToiThieu, "Học bổng: GPA học kỳ tối thiểu", "2.5", "điểm hệ 4"),
            QuyChe(MaQuyChe.HocBongSoTien, "Học bổng khuyến khích: số tiền", "1000000", "VND"));

        db.Add(new CoSoThucTap { Ma = "FPT", Ten = "Công ty Cổ phần FPT", DiaChi = "Hà Nội", NguoiLienHe = "Phòng nhân sự" });
        await db.SaveChangesAsync();

        db.Add(new ThongBao
        {
            TieuDe = $"Mở đăng ký học phần {hocKyNay.Ten} {namNay.Ten}",
            NoiDung = "Sinh viên đăng nhập và đăng ký học phần trong thời gian của đợt đăng ký.",
            NguoiTaoId = adminUser.Id,
            NgayDang = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static NamHoc TaoNamHoc(int namBatDau) => new()
    {
        Ten = $"{namBatDau}-{namBatDau + 1}",
        NgayBatDau = new DateOnly(namBatDau, 8, 15),
        NgayKetThuc = new DateOnly(namBatDau + 1, 8, 14)
    };

    // Học kỳ 1: 15/8 - 15/1; học kỳ 2: 16/1 - 30/6; học kỳ 3 (hè): 1/7 - 14/8
    private static HocKy TaoHocKy(NamHoc namHoc, int soThuTu)
    {
        var nam = namHoc.NgayBatDau.Year;
        var (batDau, ketThuc) = soThuTu switch
        {
            1 => (new DateOnly(nam, 8, 15), new DateOnly(nam + 1, 1, 15)),
            2 => (new DateOnly(nam + 1, 1, 16), new DateOnly(nam + 1, 6, 30)),
            _ => (new DateOnly(nam + 1, 7, 1), new DateOnly(nam + 1, 8, 14))
        };
        return new HocKy
        {
            Ten = soThuTu == 3 ? "Học kỳ hè" : $"Học kỳ {soThuTu}", SoThuTu = soThuTu,
            NamHocId = namHoc.Id, NgayBatDau = batDau, NgayKetThuc = ketThuc
        };
    }

    private static DiemHocPhan DiemDaKhoa(int dangKyId, decimal quaTrinh, decimal giuaKy, decimal cuoiKy)
    {
        var tongKet = GradeCalculator.TinhDiemTongKet(quaTrinh, giuaKy, cuoiKy);
        return new DiemHocPhan
        {
            DangKyHocPhanId = dangKyId, DiemQuaTrinh = quaTrinh, DiemGiuaKy = giuaKy, DiemCuoiKy = cuoiKy,
            DiemTongKet = tongKet, DiemChu = GradeCalculator.QuyDoiDiemChu(tongKet), DaKhoa = true
        };
    }

    private static TaiKhoan TaiKhoan(string tenDangNhap, string matKhau, string hoTen, VaiTro vaiTro,
        int? sinhVienId = null, int? giangVienId = null) => new()
    {
        TenDangNhap = tenDangNhap, MatKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhau), HoTen = hoTen,
        VaiTro = vaiTro, SinhVienId = sinhVienId, GiangVienId = giangVienId
    };

    private static QuyCheHocVu QuyChe(string ma, string ten, string giaTri, string donVi) =>
        new() { Ma = ma, Ten = ten, GiaTri = giaTri, DonVi = donVi, MoTa = "Giá trị mặc định khi seed; có thể chỉnh." };
}
