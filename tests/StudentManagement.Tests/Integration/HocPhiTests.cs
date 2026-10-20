using System.Net;

namespace StudentManagement.Tests.Integration;

[Collection(ApiCollection.Name)]
public class HocPhiTests(ApiFactory factory)
{
    private const string HocPhi = "/api/cong-tac-sinh-vien/hoc-phi";

    private async Task<(ApiClient Admin, ApiClient SinhVien, int HocPhiId, decimal SoTien)> TaoKhoanHocPhiAsync()
    {
        var admin = await factory.AdminAsync();
        var lopId = await TestData.TaoLopAsync(admin, "IT003");
        var (sinhVienId, sv) = await TestData.TaoSinhVienAsync(factory, admin);
        await TestData.DangKyAsync(sv, lopId);
        var hocKyId = await TestData.HocKyDangMoAsync(admin);

        await admin.DataAsync(HttpMethod.Post, $"{HocPhi}/tinh/{hocKyId}");
        var hocPhi = (await admin.DataAsync(HttpMethod.Get, $"{HocPhi}?sinhVienId={sinhVienId}&hocKyId={hocKyId}")).Items().Single();
        return (admin, sv, hocPhi.Id(), hocPhi.GetProperty("soTien").GetDecimal());
    }

    [Fact]
    public async Task TinhHocPhi_TheoTinChiNhanDonGiaQuyChe()
    {
        var (_, _, _, soTien) = await TaoKhoanHocPhiAsync();
        Assert.Equal(3 * 600_000m, soTien); // IT003 3 tín chỉ, đơn giá seed 600000
    }

    [Fact]
    public async Task NopNhieuLan_KhongVuotSoConNo()
    {
        var (admin, sv, id, soTien) = await TaoKhoanHocPhiAsync();

        var lan1 = await admin.DataAsync(HttpMethod.Put, $"{HocPhi}/{id}/thanh-toan", new { soTien = 500_000m });
        Assert.Equal(soTien - 500_000m, lan1.GetProperty("hocPhi").GetProperty("soConNo").GetDecimal());
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsync($"{HocPhi}/{id}/thanh-toan", new { soTien })).Status);

        var lan2 = await admin.DataAsync(HttpMethod.Put, $"{HocPhi}/{id}/thanh-toan", new { });
        Assert.True(lan2.GetProperty("hocPhi").GetProperty("daThanhToan").GetBoolean());
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PutAsync($"{HocPhi}/{id}/thanh-toan", new { })).Status);

        // Sinh viên thấy đủ 2 lần nộp của mình
        var lichSu = await sv.DataAsync(HttpMethod.Get, $"/api/cong-tac-sinh-vien/toi/hoc-phi/{id}/thanh-toan");
        Assert.Equal(2, lichSu.GetArrayLength());
    }

    [Fact]
    public async Task NopDongThoi_ChiMotLanThanhCong()
    {
        var (admin, _, id, soTien) = await TaoKhoanHocPhiAsync();

        var ketQua = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => admin.PutAsync($"{HocPhi}/{id}/thanh-toan", new { })));

        Assert.Equal(1, ketQua.Count(x => x.Status == HttpStatusCode.OK));
        var hocPhi = await admin.DataAsync(HttpMethod.Get, $"{HocPhi}/{id}");
        Assert.Equal(soTien, hocPhi.GetProperty("soTienDaNop").GetDecimal());
    }

    [Fact]
    public async Task SinhVienKhac_KhongXemDuocLichSuNop()
    {
        var (admin, _, id, _) = await TaoKhoanHocPhiAsync();
        var (_, nguoiKhac) = await TestData.TaoSinhVienAsync(factory, admin);

        Assert.Equal(HttpStatusCode.NotFound,
            (await nguoiKhac.GetAsync($"/api/cong-tac-sinh-vien/toi/hoc-phi/{id}/thanh-toan")).Status);
    }
}
