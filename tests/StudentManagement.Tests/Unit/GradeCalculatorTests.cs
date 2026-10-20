using StudentManagement.Common;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Tests.Unit;

public class GradeCalculatorTests
{
    [Theory]
    [InlineData(8, 7, 6.5, 6.8)]   // 1.2 + 1.05 + 4.55
    [InlineData(10, 10, 10, 10)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(10, 10, 9.5, 9.7)] // 9.65 làm tròn lên (AwayFromZero), không làm tròn về số chẵn
    [InlineData(9, 9, 9, 9)]
    public void TinhDiemTongKet_TheoTrongSo15_15_70_VaLamTron1ChuSo(
        decimal quaTrinh, decimal giuaKy, decimal cuoiKy, decimal mongDoi) =>
        Assert.Equal(mongDoi, GradeCalculator.TinhDiemTongKet(quaTrinh, giuaKy, cuoiKy));

    [Theory]
    [InlineData(10.0, "A+")] [InlineData(9.5, "A+")] [InlineData(9.4, "A")]
    [InlineData(8.5, "A")] [InlineData(8.4, "B+")] [InlineData(8.0, "B+")]
    [InlineData(7.9, "B")] [InlineData(7.0, "B")] [InlineData(6.9, "C+")]
    [InlineData(6.0, "C+")] [InlineData(5.9, "C")] [InlineData(5.5, "C")]
    [InlineData(5.4, "D+")] [InlineData(4.5, "D+")] [InlineData(4.4, "D")]
    [InlineData(4.0, "D")] [InlineData(3.9, "F+")] [InlineData(2.0, "F+")]
    [InlineData(1.9, "F")] [InlineData(0.0, "F")]
    public void QuyDoiDiemChu_DungNguongTungMuc(decimal diem, string diemChu) =>
        Assert.Equal(diemChu, GradeCalculator.QuyDoiDiemChu(diem));

    [Theory]
    [InlineData("A+", 4.0)] [InlineData("A", 3.8)] [InlineData("B+", 3.5)] [InlineData("B", 3.0)]
    [InlineData("C+", 2.5)] [InlineData("C", 2.0)] [InlineData("D+", 1.5)] [InlineData("D", 1.0)]
    [InlineData("F+", 0.5)] [InlineData("F", 0.0)]
    public void QuyDoiDiemHe4_DungBang(string diemChu, decimal he4) =>
        Assert.Equal(he4, GradeCalculator.QuyDoiDiemHe4(diemChu));

    [Fact]
    public void QuyDoiDiemHe4_DiemChuLa_BaoLoi409()
    {
        var ex = Assert.Throws<AppException>(() => GradeCalculator.QuyDoiDiemHe4("X"));
        Assert.Equal(409, ex.StatusCode);
    }

    [Theory]
    [InlineData(4.0, true)] [InlineData(3.9, false)] [InlineData(10, true)]
    public void LaDiemDat_TuBonTroLen(decimal diem, bool dat) =>
        Assert.Equal(dat, GradeCalculator.LaDiemDat(diem));

    [Theory]
    [InlineData(4.0, "XuatSac")] [InlineData(3.6, "XuatSac")] [InlineData(3.59, "Gioi")]
    [InlineData(3.2, "Gioi")] [InlineData(3.19, "Kha")] [InlineData(2.5, "Kha")]
    [InlineData(2.49, "TrungBinh")] [InlineData(2.0, "TrungBinh")] [InlineData(1.99, "Yeu")] [InlineData(0, "Yeu")]
    public void XepLoai_TheoDiemHe4(decimal diemHe4, string ma) =>
        Assert.Equal(ma, GradeCalculator.XepLoai(diemHe4).Ma);

    [Fact]
    public void TinhTrungBinh_ChuaCoTinChi_TraVeNull() =>
        Assert.Null(GradeCalculator.TinhTrungBinh(0, 0));

    [Fact]
    public void TinhTrungBinh_TrongSoTinChi_LamTron2ChuSo()
    {
        // (3.8 * 3 + 3.0 * 3) / 6 = 3.4; (4 * 3 + 3.5 * 2 + 2 * 2) / 7 = 3.2857... -> 3.29
        Assert.Equal(3.4m, GradeCalculator.TinhTrungBinh(3.8m * 3 + 3.0m * 3, 6));
        Assert.Equal(3.29m, GradeCalculator.TinhTrungBinh(4m * 3 + 3.5m * 2 + 2m * 2, 7));
    }

    [Theory]
    [InlineData(7.5, true)] [InlineData(7.25, true)] [InlineData(7.555, false)]
    public void CoToiDaHaiChuSoThapPhan(decimal diem, bool hopLe) =>
        Assert.Equal(hopLe, GradeCalculator.CoToiDaHaiChuSoThapPhan(diem));
}
