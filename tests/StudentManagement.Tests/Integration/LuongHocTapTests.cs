using System.Net;
using StudentManagement.Data;

namespace StudentManagement.Tests.Integration;

/// <summary>Luồng chính: đăng ký → bắt đầu học → giảng viên nhập/tính điểm → kết thúc → khóa → bảng điểm, phúc khảo.</summary>
[Collection(ApiCollection.Name)]
public class LuongHocTapTests(ApiFactory factory)
{
    [Fact]
    public async Task DangKy_NhapDiem_KhoaDiem_SinhVienXemVaPhucKhao()
    {
        var admin = await factory.AdminAsync();
        var pdt = await factory.PhongDaoTaoAsync();
        var gv = await factory.GiangVienAsync();
        var giangVienId = (await gv.DataAsync(HttpMethod.Get, "/api/auth/me")).GetProperty("giangVienId").GetInt32();
        var lopId = await TestData.TaoLopAsync(admin, "IT003", giangVienId);
        var (sinhVienId, sv) = await TestData.TaoSinhVienAsync(factory, admin);

        var dangKyId = await TestData.DangKyAsync(sv, lopId);
        // Lớp chưa bắt đầu học thì chưa được nhập điểm
        var nhap = new { danhSachDiem = new[] { new { dangKyHocPhanId = dangKyId, diemQuaTrinh = 8m, diemGiuaKy = 7m, diemCuoiKy = 6.5m } } };
        Assert.Equal(HttpStatusCode.Conflict, (await gv.PutAsync($"/api/diem/lop-hoc-phan/{lopId}", nhap)).Status);

        await pdt.DataAsync(HttpMethod.Post, $"/api/lop-hoc-phan/{lopId}/bat-dau-hoc");
        await gv.DataAsync(HttpMethod.Put, $"/api/diem/lop-hoc-phan/{lopId}", nhap);
        await gv.DataAsync(HttpMethod.Post, $"/api/diem/lop-hoc-phan/{lopId}/tinh-diem");
        await pdt.DataAsync(HttpMethod.Post, $"/api/lop-hoc-phan/{lopId}/ket-thuc");
        // Giảng viên không tự khóa điểm; Phòng đào tạo duyệt
        Assert.Equal(HttpStatusCode.Forbidden, (await gv.PostAsync($"/api/diem/lop-hoc-phan/{lopId}/khoa")).Status);
        await pdt.DataAsync(HttpMethod.Post, $"/api/diem/lop-hoc-phan/{lopId}/khoa");

        var bangDiemLop = await gv.DataAsync(HttpMethod.Get, $"/api/diem/lop-hoc-phan/{lopId}");
        var dong = bangDiemLop.GetProperty("danhSach").EnumerateArray().Single();
        Assert.Equal(6.8m, dong.GetProperty("diemTongKet").GetDecimal());
        Assert.Equal("C+", dong.GetProperty("diemChu").GetString());
        Assert.True(dong.GetProperty("daKhoa").GetBoolean());

        var bangDiem = await sv.DataAsync(HttpMethod.Get, "/api/diem/toi/bang-diem");
        Assert.Equal(sinhVienId, bangDiem.GetProperty("sinhVienId").GetInt32());
        Assert.Equal(2.5m, bangDiem.GetProperty("cpa").GetDecimal());

        var phucKhao = await sv.DataAsync(HttpMethod.Post, "/api/diem/toi/phuc-khao",
            new { diemHocPhanId = dong.GetProperty("diemHocPhanId").GetInt32(), lyDo = "Xin chấm lại" }, HttpStatusCode.Created);
        await pdt.DataAsync(HttpMethod.Post, $"/api/phuc-khao/{phucKhao.Id()}/xu-ly", new { chapNhan = true, diemMoi = 8m });

        bangDiemLop = await pdt.DataAsync(HttpMethod.Get, $"/api/diem/lop-hoc-phan/{lopId}");
        dong = bangDiemLop.GetProperty("danhSach").EnumerateArray().Single();
        Assert.Equal(7.9m, dong.GetProperty("diemTongKet").GetDecimal()); // 1.2 + 1.05 + 5.6 = 7.85 -> 7.9
    }

    [Fact]
    public async Task GiangVien_KhongThaoTacDuocLopKhongPhanCong()
    {
        var admin = await factory.AdminAsync();
        var gv = await factory.GiangVienAsync();
        var lopId = await TestData.TaoLopAsync(admin, "IT003");

        Assert.Equal(HttpStatusCode.Forbidden, (await gv.GetAsync($"/api/diem/lop-hoc-phan/{lopId}")).Status);
        var lopCuaToi = await gv.DataAsync(HttpMethod.Get, "/api/giang-vien/toi/lop-hoc-phan?pageSize=100");
        Assert.DoesNotContain(lopCuaToi.Items(), x => x.Id() == lopId);
    }

    [Fact]
    public async Task DangKy_ThieuTienQuyet_BiChanKemLyDo()
    {
        var admin = await factory.AdminAsync();
        var lopId = await TestData.TaoLopAsync(admin, "IT002");
        var (_, sv) = await TestData.TaoSinhVienAsync(factory, admin);

        var (status, body) = await sv.PostAsync("/api/dang-ky-hoc-phan", new { lopHocPhanId = lopId });

        Assert.Equal(HttpStatusCode.Conflict, status);
        Assert.Contains("Chưa hoàn thành môn tiên quyết: IT001", body.Message());
    }

    [Fact]
    public async Task SinhVien_KhongXemDuocHoSoNguoiKhac()
    {
        var admin = await factory.AdminAsync();
        var (_, sv) = await TestData.TaoSinhVienAsync(factory, admin);
        var sv001 = await factory.LoginAsync("sv001", DbSeeder.MatKhauSinhVien);
        var idCuaSv001 = (await sv001.DataAsync(HttpMethod.Get, "/api/auth/me")).GetProperty("sinhVienId").GetInt32();

        Assert.Equal(HttpStatusCode.Forbidden, (await sv.GetAsync($"/api/dang-ky-hoc-phan/da-dang-ky?sinhVienId={idCuaSv001}")).Status);
        Assert.Equal(HttpStatusCode.Forbidden, (await sv.GetAsync($"/api/diem/sinh-vien/{idCuaSv001}")).Status);
    }

    [Fact]
    public async Task DieuKienTotNghiep_LietKeMonBatBuocConThieu()
    {
        var pdt = await factory.PhongDaoTaoAsync();
        var sv001 = await factory.LoginAsync("sv001", DbSeeder.MatKhauSinhVien);
        var id = (await sv001.DataAsync(HttpMethod.Get, "/api/auth/me")).GetProperty("sinhVienId").GetInt32();

        var dieuKien = await pdt.DataAsync(HttpMethod.Get, $"/api/tot-nghiep/dieu-kien/{id}");

        // Seed: sv001 mới đạt IT001 trong 3 môn bắt buộc
        Assert.Equal("KhongDuDieuKien", dieuKien.GetProperty("ketLuan").GetString());
        var conThieu = dieuKien.GetProperty("monBatBuoc").GetProperty("monConThieu").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        Assert.Contains(conThieu, x => x!.StartsWith("IT003"));
        Assert.DoesNotContain(conThieu, x => x!.StartsWith("IT001"));
    }
}
