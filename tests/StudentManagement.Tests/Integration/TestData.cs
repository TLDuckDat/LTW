using System.Net;

namespace StudentManagement.Tests.Integration;

/// <summary>Tạo dữ liệu riêng cho từng test (mã ngẫu nhiên) để các test không ảnh hưởng lẫn nhau.</summary>
public static class TestData
{
    public static string Tag() => Guid.NewGuid().ToString("N")[..8];

    /// <summary>Sinh viên mới kèm tài khoản SinhVien đã liên kết; trả về Id và client đã đăng nhập.</summary>
    public static async Task<(int SinhVienId, ApiClient Client)> TaoSinhVienAsync(ApiFactory factory, ApiClient admin)
    {
        var tag = Tag();
        var nganhId = (await admin.DataAsync(HttpMethod.Get, "/api/nganh")).Items().First().Id();
        var sv = await admin.DataAsync(HttpMethod.Post, "/api/sinh-vien",
            new { mssv = "T" + tag, hoTen = "Sinh viên test " + tag, ngaySinh = "2006-01-01", nganhId }, HttpStatusCode.Created);
        await admin.DataAsync(HttpMethod.Post, "/api/auth/accounts",
            new { tenDangNhap = "t" + tag, matKhau = "Matkhau@1", hoTen = "SV " + tag, vaiTro = 3, sinhVienId = sv.Id() },
            HttpStatusCode.Created);
        return (sv.Id(), await factory.LoginAsync("t" + tag, "Matkhau@1"));
    }

    /// <summary>Học kỳ đang có đợt đăng ký mở (do seed tạo).</summary>
    public static async Task<int> HocKyDangMoAsync(ApiClient admin) =>
        (await admin.DataAsync(HttpMethod.Get, "/api/dot-dang-ky?trangThai=DangMo")).Items().First()
            .GetProperty("hocKyId").GetInt32();

    /// <summary>Lớp học phần mới của môn (theo mã) trong học kỳ đang mở đăng ký.</summary>
    public static async Task<int> TaoLopAsync(ApiClient admin, string maMon = "IT003", int? giangVienId = null)
    {
        var hocKyId = await HocKyDangMoAsync(admin);
        var monHocId = (await admin.DataAsync(HttpMethod.Get, $"/api/mon-hoc?search={maMon}")).Items()
            .First(x => x.GetProperty("ma").GetString() == maMon).Id();
        var lop = await admin.DataAsync(HttpMethod.Post, "/api/lop-hoc-phan",
            new { ma = $"{maMon}-T{Tag()}", hocKyId, monHocId, siSoToiDa = 50 }, HttpStatusCode.Created);
        if (giangVienId is not null)
            await admin.DataAsync(HttpMethod.Put, $"/api/lop-hoc-phan/{lop.Id()}/giang-vien", new { giangVienId });
        return lop.Id();
    }

    public static async Task<int> DangKyAsync(ApiClient sinhVien, int lopHocPhanId) =>
        (await sinhVien.DataAsync(HttpMethod.Post, "/api/dang-ky-hoc-phan", new { lopHocPhanId }, HttpStatusCode.Created)).Id();
}
