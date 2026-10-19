using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Entities;

namespace StudentManagement.Modules.Graduation.Dtos;

public class XetTotNghiepQuery : PagedQuery
{
    public KetQuaXetTotNghiep? KetQua { get; set; }
    public bool? DaTotNghiep { get; set; }
}

public record XetTotNghiepDto(int Id, int SinhVienId, string MaSinhVien,
    string HoTen, string TrangThaiSinhVien, DateOnly NgayXet,
    int? TongTinChiTichLuy, decimal? DiemTrungBinhTichLuy,
    string? XepLoai, string KetQua, string? LyDo);

public record ChiTietXetTotNghiepDto(XetTotNghiepDto HienTai,
    IReadOnlyList<XetTotNghiepDto> LichSu);
