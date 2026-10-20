using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StudentManagement.Tests.Integration;

/// <summary>Chống dò mật khẩu, refresh token (xoay vòng, phát hiện dùng lại, đăng xuất) và nhật ký hoạt động.</summary>
[Collection(ApiCollection.Name)]
public class SecurityTests(ApiFactory factory)
{
    private const string MatKhau = "Matkhau@1";

    private async Task<(string TenDangNhap, int Id)> TaoTaiKhoanAsync(ApiClient admin, int vaiTro = 1)
    {
        var ten = "u" + TestData.Tag();
        var data = await admin.DataAsync(HttpMethod.Post, "/api/auth/accounts",
            new { tenDangNhap = ten, matKhau = MatKhau, hoTen = "Test " + ten, vaiTro }, HttpStatusCode.Created);
        return (ten, data.Id());
    }

    private Task<(HttpStatusCode Status, JsonElement Body)> DangNhapAsync(string tenDangNhap, string matKhau) =>
        factory.Anonymous().PostAsync("/api/auth/login", new { tenDangNhap, matKhau });

    private static ApiClient VoiToken(ApiFactory factory, JsonElement phien)
    {
        var client = factory.Anonymous();
        client.Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", phien.GetProperty("token").GetString());
        return client;
    }

    private static string Refresh(JsonElement phien) => phien.GetProperty("refreshToken").GetString()!;

    // ===== Chống dò mật khẩu =====

    [Fact]
    public async Task SaiNamLan_LanThuSauBiChan429_KeCaKhiDungMatKhau()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);

        for (var lan = 1; lan <= 5; lan++)
        {
            var (status, body) = await DangNhapAsync(ten, "sai-mat-khau");
            Assert.Equal(HttpStatusCode.Unauthorized, status);
            if (lan < 5) Assert.Contains($"Còn {5 - lan} lần thử", body.Message());
        }

        var (chan, chanBody) = await DangNhapAsync(ten, MatKhau);
        Assert.Equal(HttpStatusCode.TooManyRequests, chan);
        Assert.Contains("thử lại sau", chanBody.Message());
        Assert.False(chanBody.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task TenDangNhapKhongTonTai_CungBiChan_KhongLoTaiKhoan()
    {
        var ten = "khongco" + TestData.Tag();
        for (var lan = 0; lan < 5; lan++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await DangNhapAsync(ten, "x")).Status);

        Assert.Equal(HttpStatusCode.TooManyRequests, (await DangNhapAsync(ten, "x")).Status);
    }

    [Fact]
    public async Task DangNhapDung_XoaBoDem()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        for (var lan = 0; lan < 4; lan++) await DangNhapAsync(ten, "sai");

        Assert.Equal(HttpStatusCode.OK, (await DangNhapAsync(ten, MatKhau)).Status);
        var (_, body) = await DangNhapAsync(ten, "sai");
        Assert.Contains("Còn 4 lần thử", body.Message());
    }

    [Fact]
    public async Task AdminDatLaiMatKhau_GoChanNgay()
    {
        var admin = await factory.AdminAsync();
        var (ten, id) = await TaoTaiKhoanAsync(admin);
        for (var lan = 0; lan < 5; lan++) await DangNhapAsync(ten, "sai");
        Assert.Equal(HttpStatusCode.TooManyRequests, (await DangNhapAsync(ten, MatKhau)).Status);

        await admin.DataAsync(HttpMethod.Post, $"/api/auth/accounts/{id}/reset-password", new { matKhauMoi = "Matkhau@2" });

        Assert.Equal(HttpStatusCode.OK, (await DangNhapAsync(ten, "Matkhau@2")).Status);
    }

    // ===== Refresh token =====

    [Fact]
    public async Task DangNhap_TraVeRefreshToken_AccessTokenNganHan()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        var (_, body) = await DangNhapAsync(ten, MatKhau);
        var phien = body.GetProperty("data");

        Assert.False(string.IsNullOrEmpty(Refresh(phien)));
        var accessHetHan = phien.GetProperty("expiresAt").GetDateTime();
        var refreshHetHan = phien.GetProperty("refreshTokenExpiresAt").GetDateTime();
        Assert.InRange((accessHetHan - DateTime.UtcNow).TotalMinutes, 55, 61);
        Assert.InRange((refreshHetHan - DateTime.UtcNow).TotalDays, 6.9, 7.01);
    }

    [Fact]
    public async Task Refresh_XoayVong_TokenCuHetHieuLuc()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        var phien1 = (await DangNhapAsync(ten, MatKhau)).Body.GetProperty("data");

        var phien2 = await factory.Anonymous().DataAsync(HttpMethod.Post, "/api/auth/refresh", new { refreshToken = Refresh(phien1) });
        Assert.NotEqual(Refresh(phien1), Refresh(phien2));
        Assert.Equal(HttpStatusCode.OK, (await VoiToken(factory, phien2).GetAsync("/api/auth/me")).Status);

        // Chỉ dùng được một lần
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.Anonymous().PostAsync("/api/auth/refresh", new { refreshToken = Refresh(phien1) })).Status);
    }

    [Fact]
    public async Task Refresh_TokenCuBiDungLai_ThuHoiMoiPhien()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        var phien1 = (await DangNhapAsync(ten, MatKhau)).Body.GetProperty("data");
        var phien2 = await factory.Anonymous().DataAsync(HttpMethod.Post, "/api/auth/refresh", new { refreshToken = Refresh(phien1) });

        // Kẻ gian dùng lại token đã xoay vòng -> phiên hợp lệ hiện tại (phien2) cũng bị thu hồi
        await factory.Anonymous().PostAsync("/api/auth/refresh", new { refreshToken = Refresh(phien1) });

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.Anonymous().PostAsync("/api/auth/refresh", new { refreshToken = Refresh(phien2) })).Status);
        var nhatKy = await admin.DataAsync(HttpMethod.Get, $"/api/auth/activity-logs?hanhDong=RefreshTaiSuDung&search={ten}");
        Assert.Single(nhatKy.Items());
    }

    [Fact]
    public async Task Logout_RefreshTokenKhongDungDuocNua()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        var phien = (await DangNhapAsync(ten, MatKhau)).Body.GetProperty("data");

        await factory.Anonymous().DataAsync(HttpMethod.Post, "/api/auth/logout", new { refreshToken = Refresh(phien) });
        await factory.Anonymous().DataAsync(HttpMethod.Post, "/api/auth/logout", new { refreshToken = Refresh(phien) }); // gọi lại vẫn OK

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.Anonymous().PostAsync("/api/auth/refresh", new { refreshToken = Refresh(phien) })).Status);
    }

    [Fact]
    public async Task LogoutAll_VoHieuCaAccessTokenVaRefreshToken()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        var mayA = (await DangNhapAsync(ten, MatKhau)).Body.GetProperty("data");
        var mayB = (await DangNhapAsync(ten, MatKhau)).Body.GetProperty("data");

        await VoiToken(factory, mayA).DataAsync(HttpMethod.Post, "/api/auth/logout-all");

        Assert.Equal(HttpStatusCode.Unauthorized, (await VoiToken(factory, mayB).GetAsync("/api/auth/me")).Status);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.Anonymous().PostAsync("/api/auth/refresh", new { refreshToken = Refresh(mayB) })).Status);
    }

    [Fact]
    public async Task TaiKhoanBiKhoaHoacDoiMatKhau_RefreshTokenHetHieuLuc()
    {
        var admin = await factory.AdminAsync();
        var (ten, id) = await TaoTaiKhoanAsync(admin);
        var phien = (await DangNhapAsync(ten, MatKhau)).Body.GetProperty("data");

        await admin.DataAsync(HttpMethod.Post, $"/api/auth/accounts/{id}/lock");

        var (status, body) = await factory.Anonymous().PostAsync("/api/auth/refresh", new { refreshToken = Refresh(phien) });
        Assert.Equal(HttpStatusCode.Unauthorized, status);
        Assert.Contains("tài khoản đã thay đổi", body.Message());
    }

    [Fact]
    public async Task RefreshTokenBia_Tra401() =>
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.Anonymous().PostAsync("/api/auth/refresh", new { refreshToken = "khong-hop-le" })).Status);

    // ===== Nhật ký hoạt động =====

    [Fact]
    public async Task ThaoTacGhi_DuocGhiNhatKy_VoiNguoiThucHienVaMaPhanHoi()
    {
        var admin = await factory.AdminAsync();
        var me = await admin.DataAsync(HttpMethod.Get, "/api/auth/me");
        var ma = "K" + TestData.Tag();
        var khoa = await admin.DataAsync(HttpMethod.Post, "/api/khoa", new { ma, ten = "Khoa nhật ký" }, HttpStatusCode.Created);
        await admin.DataAsync(HttpMethod.Delete, $"/api/khoa/{khoa.Id()}");

        var log = await admin.DataAsync(HttpMethod.Get, $"/api/auth/activity-logs?hanhDong=Khoa.Delete&search=/api/khoa/{khoa.Id()}");
        var dong = log.Items().Single();
        Assert.Equal(me.Id(), dong.GetProperty("taiKhoanId").GetInt32());
        Assert.Equal("DELETE", dong.GetProperty("phuongThuc").GetString());
        Assert.Equal(200, dong.GetProperty("maPhanHoi").GetInt32());
        Assert.Contains($"id={khoa.Id()}", dong.GetProperty("chiTiet").GetString());
        Assert.True(dong.GetProperty("thanhCong").GetBoolean());
    }

    [Fact]
    public async Task ThaoTacBiTuChoi403_CungDuocGhi()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin, vaiTro: 2);
        var gv = await factory.LoginAsync(ten, MatKhau);

        Assert.Equal(HttpStatusCode.Forbidden, (await gv.PostAsync("/api/khoa", new { ma = "X", ten = "X" })).Status);

        var log = await admin.DataAsync(HttpMethod.Get, $"/api/auth/activity-logs?search={ten}&hanhDong=Khoa.Create");
        var dong = log.Items().Single();
        Assert.Equal(403, dong.GetProperty("maPhanHoi").GetInt32());
        Assert.False(dong.GetProperty("thanhCong").GetBoolean());
    }

    [Fact]
    public async Task DoiVaiTro_GhiChiTietCuMoi()
    {
        var admin = await factory.AdminAsync();
        var (ten, id) = await TaoTaiKhoanAsync(admin, vaiTro: 3);

        await admin.DataAsync(HttpMethod.Put, $"/api/auth/accounts/{id}/role", new { vaiTro = 2 });

        var log = await admin.DataAsync(HttpMethod.Get, $"/api/auth/activity-logs?hanhDong=Auth.ChangeRole&search={ten}");
        Assert.Contains("vai trò SinhVien -> GiangVien", log.Items().Single().GetProperty("chiTiet").GetString());
    }

    [Fact]
    public async Task DoiMatKhau_KhongLuuMatKhauVaoNhatKy()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        var client = await factory.LoginAsync(ten, MatKhau);
        const string matKhauMoi = "BiMat@98765";

        await client.DataAsync(HttpMethod.Post, "/api/auth/change-password", new { matKhauCu = MatKhau, matKhauMoi });

        var log = await admin.DataAsync(HttpMethod.Get, $"/api/auth/activity-logs?search={ten}&pageSize=100");
        Assert.Contains(log.Items(), x => x.GetProperty("hanhDong").GetString() == "Auth.ChangePassword");
        Assert.DoesNotContain(matKhauMoi, log.GetRawText());
        Assert.DoesNotContain(MatKhau, log.GetRawText());
    }

    [Fact]
    public async Task DangNhapSai_VaBiChan_DuocGhiNhatKy()
    {
        var admin = await factory.AdminAsync();
        var (ten, _) = await TaoTaiKhoanAsync(admin);
        for (var lan = 0; lan < 6; lan++) await DangNhapAsync(ten, "sai");

        var log = await admin.DataAsync(HttpMethod.Get, $"/api/auth/activity-logs?search={ten}&thanhCong=false&pageSize=100");
        Assert.Equal(5, log.Items().Count(x => x.GetProperty("hanhDong").GetString() == "Login"));
        Assert.Equal(1, log.Items().Count(x => x.GetProperty("hanhDong").GetString() == "LoginBiChan"));
    }

    [Fact]
    public async Task NopHocPhi_GhiSoTien()
    {
        var admin = await factory.AdminAsync();
        var lopId = await TestData.TaoLopAsync(admin, "IT003");
        var (sinhVienId, sv) = await TestData.TaoSinhVienAsync(factory, admin);
        await TestData.DangKyAsync(sv, lopId);
        var hocKyId = await TestData.HocKyDangMoAsync(admin);
        await admin.DataAsync(HttpMethod.Post, $"/api/cong-tac-sinh-vien/hoc-phi/tinh/{hocKyId}");
        var hocPhi = (await admin.DataAsync(HttpMethod.Get,
            $"/api/cong-tac-sinh-vien/hoc-phi?sinhVienId={sinhVienId}&hocKyId={hocKyId}")).Items().Single();

        await admin.DataAsync(HttpMethod.Post, $"/api/cong-tac-sinh-vien/hoc-phi/{hocPhi.Id()}/thanh-toan",
            new { soTien = 500000, hinhThuc = "Tiền mặt" }, HttpStatusCode.Created);

        var log = await admin.DataAsync(HttpMethod.Get,
            $"/api/auth/activity-logs?hanhDong=HocPhi.ThanhToan&search=/hoc-phi/{hocPhi.Id()}/");
        var chiTiet = log.Items().Single().GetProperty("chiTiet").GetString();
        Assert.Contains("nộp 500000", chiTiet);
        Assert.Contains("hình thức Tiền mặt", chiTiet);
    }
}
