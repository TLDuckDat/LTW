using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using StudentManagement.Data;

namespace StudentManagement.Tests.Integration;

/// <summary>Quy ước REST chung: định dạng lỗi, 201 + Location, enum dạng chuỗi, thao tác idempotent.</summary>
[Collection(ApiCollection.Name)]
public class QuyUocRestTests(ApiFactory factory)
{
    [Fact]
    public async Task LoiKhongDoControllerTraVe_VanDungDinhDangApiResponse()
    {
        var admin = await factory.AdminAsync();
        var sv = await factory.LoginAsync("sv001", DbSeeder.MatKhauSinhVien);

        await KiemTraLoi(factory.Anonymous().GetAsync("/api/khoa"), HttpStatusCode.Unauthorized);
        await KiemTraLoi(sv.GetAsync("/api/sinh-vien"), HttpStatusCode.Forbidden);
        await KiemTraLoi(admin.GetAsync("/api/khong-ton-tai"), HttpStatusCode.NotFound);
        await KiemTraLoi(admin.SendAsync(HttpMethod.Patch, "/api/khoa"), HttpStatusCode.MethodNotAllowed);

        using var vanBan = await admin.Http.PostAsync("/api/khoa", new StringContent("x", Encoding.UTF8, "text/plain"));
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, vanBan.StatusCode);
        Assert.Contains("\"success\":false", await vanBan.Content.ReadAsStringAsync());
    }

    private static async Task KiemTraLoi(Task<(HttpStatusCode Status, JsonElement Body)> goi, HttpStatusCode mongDoi)
    {
        var (status, body) = await goi;
        Assert.Equal(mongDoi, status);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.False(string.IsNullOrEmpty(body.Message()));
    }

    [Fact]
    public async Task TaoMoi_Tra201KemLocationDocDuoc()
    {
        var admin = await factory.AdminAsync();

        using var response = await admin.Http.PostAsync("/api/khoa",
            JsonContent.Create(new { ma = "L" + TestData.Tag()[..6], ten = "Khoa test Location" }));
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").Id();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/khoa/{id}", response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(response.Headers.Location!.OriginalString)).Status);
    }

    [Fact]
    public async Task Enum_NhanTenChuoiVaTraVeTenChuoi()
    {
        var admin = await factory.AdminAsync();

        var taiKhoan = await admin.DataAsync(HttpMethod.Post, "/api/auth/accounts",
            new { tenDangNhap = "e" + TestData.Tag(), matKhau = "Matkhau@1", hoTen = "Enum", vaiTro = "PhongDaoTao" },
            HttpStatusCode.Created);

        Assert.Equal("PhongDaoTao", taiKhoan.GetProperty("vaiTro").GetString());
    }

    [Fact]
    public async Task KhoaVaMoKhoa_GoiLaiNhieuLanVanCungKetQua()
    {
        var admin = await factory.AdminAsync();
        var id = (await admin.DataAsync(HttpMethod.Post, "/api/auth/accounts",
            new { tenDangNhap = "k" + TestData.Tag(), matKhau = "Matkhau@1", hoTen = "Khoa", vaiTro = "GiangVien" },
            HttpStatusCode.Created)).Id();

        for (var lan = 0; lan < 2; lan++)
            Assert.True((await admin.DataAsync(HttpMethod.Post, $"/api/auth/accounts/{id}/lock")).GetProperty("daKhoa").GetBoolean());
        for (var lan = 0; lan < 2; lan++)
            Assert.False((await admin.DataAsync(HttpMethod.Post, $"/api/auth/accounts/{id}/unlock")).GetProperty("daKhoa").GetBoolean());
    }
}
