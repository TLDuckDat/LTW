using System.Net;
using StudentManagement.Data;

namespace StudentManagement.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthAndErrorTests(ApiFactory factory)
{
    [Theory]
    [InlineData("admin", DbSeeder.MatKhauAdmin, "Admin")]
    [InlineData("pdt", DbSeeder.MatKhauPhongDaoTao, "PhongDaoTao")]
    [InlineData("gv001", DbSeeder.MatKhauGiangVien, "GiangVien")]
    [InlineData("sv001", DbSeeder.MatKhauSinhVien, "SinhVien")]
    public async Task TaiKhoanSeed_DangNhapDuocVoiDungVaiTro(string tenDangNhap, string matKhau, string vaiTro)
    {
        var (status, body) = await factory.Anonymous().PostAsync("/api/auth/login", new { tenDangNhap, matKhau });

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(vaiTro, body.GetProperty("data").GetProperty("vaiTro").GetString());
    }

    [Fact]
    public async Task SaiMatKhau_Tra401DangApiResponse()
    {
        var (status, body) = await factory.Anonymous().PostAsync("/api/auth/login", new { tenDangNhap = "admin", matKhau = "sai-mat-khau" });

        Assert.Equal(HttpStatusCode.Unauthorized, status);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.StartsWith("Tên đăng nhập hoặc mật khẩu không chính xác.", body.Message());
    }

    // Hai DTO trùng tên ở hai module làm Swagger lỗi 500 lúc chạy, dù build vẫn thành công
    [Fact]
    public async Task SwaggerJson_SinhDuocTaiLieu()
    {
        var (status, body) = await factory.Anonymous().GetAsync("/swagger/v1/swagger.json");

        Assert.True(status == HttpStatusCode.OK, $"swagger.json trả {(int)status}: {body}");
        Assert.True(body.GetProperty("paths").EnumerateObject().Any());
    }

    [Fact]
    public async Task KhongCoToken_Tra401() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.Anonymous().GetAsync("/api/khoa")).Status);

    [Fact]
    public async Task SinhVien_KhongDuocGhiDanhMuc_NhungDuocDoc()
    {
        var sv = await factory.LoginAsync("sv001", DbSeeder.MatKhauSinhVien);

        Assert.Equal(HttpStatusCode.OK, (await sv.GetAsync("/api/khoa")).Status);
        Assert.Equal(HttpStatusCode.Forbidden, (await sv.PostAsync("/api/khoa", new { ma = "X", ten = "X" })).Status);
        Assert.Equal(HttpStatusCode.Forbidden, (await sv.GetAsync("/api/sinh-vien")).Status);
    }

    [Fact]
    public async Task KhongTimThay_Tra404VoiThongBaoChuan()
    {
        var admin = await factory.AdminAsync();
        var (status, body) = await admin.GetAsync("/api/khoa/999999");

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Equal("Không tìm thấy khoa.", body.Message());
    }

    [Fact]
    public async Task DuLieuSaiRangBuoc_Tra400()
    {
        var admin = await factory.AdminAsync();
        var (status, body) = await admin.PostAsync("/api/khoa", new { ma = "", ten = "" });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.StartsWith("Dữ liệu đầu vào không hợp lệ", body.Message());
    }

    [Fact]
    public async Task ResetMatKhau_ThuHoiTokenCu()
    {
        var admin = await factory.AdminAsync();
        var (_, sv) = await TestData.TaoSinhVienAsync(factory, admin);
        var me = await sv.DataAsync(HttpMethod.Get, "/api/auth/me");

        await admin.DataAsync(HttpMethod.Put, $"/api/auth/accounts/{me.Id()}/reset-password", new { matKhauMoi = "Matkhau@2" });

        Assert.Equal(HttpStatusCode.Unauthorized, (await sv.GetAsync("/api/auth/me")).Status);
    }
}
