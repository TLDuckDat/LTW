using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Tests.Unit;

public class PagingTests
{
    [Theory]
    [InlineData(0, 1)] [InlineData(-5, 1)] [InlineData(3, 3)]
    public void PagedQuery_TrangNhoHon1_VeTrang1(int page, int mongDoi) =>
        Assert.Equal(mongDoi, new PagedQuery { Page = page }.Page);

    [Theory]
    [InlineData(0, 20)] [InlineData(101, 20)] [InlineData(100, 100)] [InlineData(5, 5)]
    public void PagedQuery_KichThuocNgoai1Den100_VeMacDinh20(int pageSize, int mongDoi) =>
        Assert.Equal(mongDoi, new PagedQuery { PageSize = pageSize }.PageSize);

    [Fact]
    public void ToPagedResult_CatDungTrang_VaTinhTongSoTrang()
    {
        var ketQua = new[] { 1, 2, 3, 4, 5 }.ToPagedResult(new PagedQuery { Page = 2, PageSize = 2 });

        Assert.Equal([3, 4], ketQua.Items);
        Assert.Equal(5, ketQua.TotalCount);
        Assert.Equal(3, ketQua.TotalPages);
    }

    [Fact]
    public void ToPagedResult_TrangVuotQua_TraVeRong() =>
        Assert.Empty(new[] { 1, 2 }.ToPagedResult(new PagedQuery { Page = 5, PageSize = 2 }).Items);
}

public class AppExceptionTests
{
    [Fact]
    public void NotFound_TaoThongBaoChuan()
    {
        var ex = AppException.NotFound("sinh viên");
        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("Không tìm thấy sinh viên.", ex.Message);
    }

    [Fact]
    public void CacMaLoi_DungStatusCode()
    {
        Assert.Equal(400, AppException.BadRequest("x").StatusCode);
        Assert.Equal(401, AppException.Unauthorized("x").StatusCode);
        Assert.Equal(403, AppException.Forbidden("x").StatusCode);
        Assert.Equal(409, AppException.Conflict("x").StatusCode);
    }
}

public class XepLoaiRenLuyenTests
{
    [Theory]
    [InlineData(100, "Xuất sắc")] [InlineData(90, "Xuất sắc")] [InlineData(89, "Tốt")] [InlineData(80, "Tốt")]
    [InlineData(79, "Khá")] [InlineData(65, "Khá")] [InlineData(64, "Trung bình")] [InlineData(50, "Trung bình")]
    [InlineData(49, "Yếu")] [InlineData(35, "Yếu")] [InlineData(34, "Kém")] [InlineData(0, "Kém")]
    public void XepLoai_TheoThangDiemRenLuyen(int diem, string xepLoai) =>
        Assert.Equal(xepLoai, StudentAffairsQueries.XepLoaiRenLuyen(diem));
}
