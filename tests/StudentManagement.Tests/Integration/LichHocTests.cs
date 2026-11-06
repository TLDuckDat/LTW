using System.Net;

namespace StudentManagement.Tests.Integration;

[Collection(ApiCollection.Name)]
public class LichHocTests(ApiFactory factory)
{
    [Fact]
    public async Task DanhSachLichHoc_SapXepTheoThuVaTiet_KemMaPhong()
    {
        var admin = await factory.AdminAsync();
        var lopId = await TestData.TaoLopAsync(admin);
        var maPhong = "P" + TestData.Tag();
        var phongId = (await admin.DataAsync(HttpMethod.Post, "/api/phong-hoc", new { ma = maPhong, sucChua = 60 }, HttpStatusCode.Created)).Id();
        var url = $"/api/lop-hoc-phan/{lopId}/lich-hoc";

        await admin.DataAsync(HttpMethod.Post, url, new { thu = 5, tietBatDau = 1, soTiet = 3, phongHocId = phongId }, HttpStatusCode.Created);
        await admin.DataAsync(HttpMethod.Post, url, new { thu = 2, tietBatDau = 7, soTiet = 3, phongHocId = phongId }, HttpStatusCode.Created);
        await admin.DataAsync(HttpMethod.Post, url, new { thu = 2, tietBatDau = 1, soTiet = 3, phongHocId = phongId }, HttpStatusCode.Created);

        var lich = (await admin.DataAsync(HttpMethod.Get, url)).EnumerateArray().ToList();

        Assert.Equal(
            new[] { (2, 1), (2, 7), (5, 1) },
            lich.Select(x => (x.GetProperty("thu").GetInt32(), x.GetProperty("tietBatDau").GetInt32())).ToArray());
        Assert.All(lich, x => Assert.Equal(maPhong, x.GetProperty("maPhong").GetString()));
    }
}
