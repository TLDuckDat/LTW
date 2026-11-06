using System.Net;

namespace StudentManagement.Tests.Integration;

/// <summary>Kiểm tra dữ liệu đầu vào và ràng buộc thời gian khi thêm mới (sinh viên, đợt đăng ký, cơ sở thực tập, lịch học).</summary>
[Collection(ApiCollection.Name)]
public class KiemTraNhapLieuTests(ApiFactory factory)
{
    private static string MssvHopLe() => "1" + Random.Shared.Next(100000000, 999999999);

    [Theory]
    [InlineData("12345")]
    [InlineData("T1234567890")]
    public async Task SinhVien_MssvSai_BiChan(string mssv)
    {
        var admin = await factory.AdminAsync();
        var nganhId = (await admin.DataAsync(HttpMethod.Get, "/api/nganh")).Items().First().Id();

        var (status, _) = await admin.PostAsync("/api/sinh-vien", new { mssv, hoTen = "Sinh viên test", ngaySinh = "2006-01-01", nganhId });

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Fact]
    public async Task SinhVien_DuoiMuoiLamTuoi_BiChan()
    {
        var admin = await factory.AdminAsync();
        var nganhId = (await admin.DataAsync(HttpMethod.Get, "/api/nganh")).Items().First().Id();
        var ngaySinh = DateTime.Today.AddYears(-10).ToString("yyyy-MM-dd");

        var (status, _) = await admin.PostAsync("/api/sinh-vien", new { mssv = MssvHopLe(), hoTen = "Sinh viên test", ngaySinh, nganhId });

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Fact]
    public async Task DotDangKy_ThoiGianDaQua_BiChan()
    {
        var admin = await factory.AdminAsync();
        var hocKyId = await TestData.HocKyDangMoAsync(admin);
        var quaKhu = DateTime.UtcNow.AddDays(-30);

        var (status, _) = await admin.PostAsync("/api/dot-dang-ky",
            new { ten = "Đợt test " + TestData.Tag(), hocKyId, ngayBatDau = quaKhu, ngayKetThuc = quaKhu.AddDays(1) });

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Fact]
    public async Task DotDangKy_ThoiGianTuongLai_DuocTao()
    {
        var admin = await factory.AdminAsync();
        var hocKyId = await TestData.HocKyDangMoAsync(admin);
        var tuongLai = DateTime.UtcNow.AddDays(30);

        var (status, _) = await admin.PostAsync("/api/dot-dang-ky",
            new { ten = "Đợt test " + TestData.Tag(), hocKyId, ngayBatDau = tuongLai, ngayKetThuc = tuongLai.AddDays(7) });

        Assert.Equal(HttpStatusCode.Created, status);
    }

    [Fact]
    public async Task LichHoc_DanhSachSapXepTheoThuVaTiet()
    {
        var admin = await factory.AdminAsync();
        var lopId = await TestData.TaoLopAsync(admin, "IT003");
        var phongId = (await admin.DataAsync(HttpMethod.Get, "/api/phong-hoc?pageSize=1")).Items().First().Id();
        var url = $"/api/lop-hoc-phan/{lopId}/lich-hoc";

        await admin.DataAsync(HttpMethod.Post, url, new { thu = 5, tietBatDau = 1, soTiet = 3, phongHocId = phongId }, HttpStatusCode.Created);
        await admin.DataAsync(HttpMethod.Post, url, new { thu = 3, tietBatDau = 7, soTiet = 3, phongHocId = phongId }, HttpStatusCode.Created);

        var thuCacBuoi = (await admin.DataAsync(HttpMethod.Get, url)).EnumerateArray().Select(x => x.GetProperty("thu").GetInt32()).ToList();
        Assert.Equal(new[] { 3, 5 }, thuCacBuoi);
    }

    [Fact]
    public async Task CoSoThucTap_MaChuaKyTuLa_BiChan()
    {
        var admin = await factory.AdminAsync();

        var (status, _) = await admin.PostAsync("/api/tot-nghiep/co-so-thuc-tap",
            new { ma = "TEST 01 !!", ten = "Cơ sở test", soDienThoai = "0912345678" });

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }
}
