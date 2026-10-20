using System.Net;

namespace StudentManagement.Tests.Integration;

/// <summary>Quy ước thời gian UTC, đăng ký đồng thời và các API nhận dữ liệu qua body.</summary>
[Collection(ApiCollection.Name)]
public class ThoiGianVaDongThoiTests(ApiFactory factory)
{
    [Fact]
    public async Task DotDangKy_LuuVaTraVeTheoUtc_GioKhongKemMuiGioLaGioVietNam()
    {
        var admin = await factory.AdminAsync();
        var hocKyId = await TestData.HocKyDangMoAsync(admin);

        var dot = await admin.DataAsync(HttpMethod.Post, "/api/dot-dang-ky", new
        {
            ten = "Đợt test giờ " + TestData.Tag(),
            hocKyId,
            ngayBatDau = "2030-01-01T08:00:00",
            ngayKetThuc = "2030-01-02T08:00:00+07:00"
        }, HttpStatusCode.Created);

        Assert.Equal("2030-01-01T01:00:00Z", dot.GetProperty("ngayBatDau").GetString());
        Assert.Equal("2030-01-02T01:00:00Z", dot.GetProperty("ngayKetThuc").GetString());
        await admin.DataAsync(HttpMethod.Delete, $"/api/dot-dang-ky/{dot.Id()}");
    }

    [Fact]
    public async Task HaiSinhVienCungGianhChoCuoi_MotNguoiDuoc_NguoiKiaNhan409()
    {
        var admin = await factory.AdminAsync();
        var lopId = await TestData.TaoLopAsync(admin);
        await admin.DataAsync(HttpMethod.Put, $"/api/dang-ky-hoc-phan/si-so/{lopId}", new { siSoToiDa = 1 });
        var (_, sv1) = await TestData.TaoSinhVienAsync(factory, admin);
        var (_, sv2) = await TestData.TaoSinhVienAsync(factory, admin);

        var ketQua = await Task.WhenAll(
            sv1.PostAsync("/api/dang-ky-hoc-phan", new { lopHocPhanId = lopId }),
            sv2.PostAsync("/api/dang-ky-hoc-phan", new { lopHocPhanId = lopId }));

        Assert.Equal([HttpStatusCode.Created, HttpStatusCode.Conflict],
            ketQua.Select(x => x.Status).Order().ToArray());
    }

    [Fact]
    public async Task ChuyenLopVaQuayLaiHoc_NhanDuLieuQuaBody()
    {
        var admin = await factory.AdminAsync();
        var (sinhVienId, _) = await TestData.TaoSinhVienAsync(factory, admin);

        await admin.DataAsync(HttpMethod.Put, $"/api/sinh-vien/{sinhVienId}/lop-hanh-chinh", new { lopHanhChinhId = (int?)null });
        await admin.DataAsync(HttpMethod.Post, $"/api/sinh-vien/{sinhVienId}/bao-luu", new { lyDo = "Nghỉ ốm" });
        await admin.DataAsync(HttpMethod.Post, $"/api/sinh-vien/{sinhVienId}/quay-lai-hoc", new { ghiChu = "Đã khỏi bệnh" });

        var lichSu = await admin.DataAsync(HttpMethod.Get, $"/api/sinh-vien/{sinhVienId}/lich-su-hoc-tap");
        Assert.Contains(lichSu.EnumerateArray(), x => x.GetProperty("ghiChu").GetString() == "Đã khỏi bệnh");
    }
}
