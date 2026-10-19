using Microsoft.EntityFrameworkCore;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Data;

/// <summary>Seed dữ liệu mẫu khi chạy ở môi trường Development (chỉ chạy khi CSDL còn trống).</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var adminUser = await db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.TenDangNhap == "admin");
        if (adminUser != null && adminUser.MatKhauHash == "CHANGE_ME")
        {
            adminUser.MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            await db.SaveChangesAsync();
        }

        if (await db.Set<Khoa>().AnyAsync()) return;

        // AUTH: Hash mật khẩu bằng BCrypt
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        if (adminUser == null)
        {
            db.Add(new TaiKhoan { TenDangNhap = "admin", MatKhauHash = adminPasswordHash, HoTen = "Quản trị viên", VaiTro = VaiTro.Admin });
        }


        var cntt = new Khoa { Ma = "CNTT", Ten = "Khoa Công nghệ thông tin" };
        var kt = new Khoa { Ma = "KT", Ten = "Khoa Kinh tế" };
        db.AddRange(cntt, kt);
        await db.SaveChangesAsync();

        var boMon = new BoMon { Ma = "KTPM", Ten = "Bộ môn Kỹ thuật phần mềm", KhoaId = cntt.Id };
        var nganh = new Nganh { Ma = "7480201", Ten = "Công nghệ thông tin", KhoaId = cntt.Id };
        var namHoc = new NamHoc { Ten = "2025-2026", NgayBatDau = new DateOnly(2025, 9, 1), NgayKetThuc = new DateOnly(2026, 8, 31) };
        db.AddRange(boMon, nganh, namHoc);
        db.AddRange(
            new MonHoc { Ma = "IT001", Ten = "Nhập môn lập trình", SoTinChi = 3 },
            new MonHoc { Ma = "IT002", Ten = "Lập trình hướng đối tượng", SoTinChi = 3 },
            new PhongHoc { Ma = "A101", SucChua = 60 });
        await db.SaveChangesAsync();

        db.Add(new HocKy { Ten = "Học kỳ 1", SoThuTu = 1, NamHocId = namHoc.Id, NgayBatDau = new DateOnly(2025, 9, 1), NgayKetThuc = new DateOnly(2026, 1, 15) });
        var gv = new GiangVien { Ma = "GV001", HoTen = "Nguyễn Văn An", HocVi = "Thạc sĩ", BoMonId = boMon.Id };
        db.Add(gv);
        await db.SaveChangesAsync();

        var lop = new LopHanhChinh { Ma = "CNTT2025A", Ten = "CNTT K2025 A", NganhId = nganh.Id, KhoaTuyen = 2025, CoVanId = gv.Id };
        db.Add(lop);
        await db.SaveChangesAsync();

        db.AddRange(
            new SinhVien { MSSV = "2025000001", HoTen = "Trần Thị Bình", NgaySinh = new DateOnly(2007, 3, 12), Email = "binh@example.com", NganhId = nganh.Id, LopHanhChinhId = lop.Id },
            new SinhVien { MSSV = "2025000002", HoTen = "Lê Văn Cường", NgaySinh = new DateOnly(2007, 7, 5), Email = "cuong@example.com", NganhId = nganh.Id, LopHanhChinhId = lop.Id });
        await db.SaveChangesAsync();
    }
}
