using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Graduation.Dtos;

public record HoiDongRequest(
    [Required, MaxLength(200)] string Ten,
    DateTime NgayBaoVe,
    [Range(1, int.MaxValue)] int ChuTichId,
    [Range(1, int.MaxValue)] int? ThuKyId);

public record HoiDongDto(int Id, string Ten, DateTime NgayBaoVe,
    int ChuTichId, string TenChuTich, int? ThuKyId, string? TenThuKy, int SoDeTai);

public record DeTaiHoiDongDto(int Id, string TenDeTai, int? SinhVienId,
    string? MaSinhVien, string? HoTenSinhVien, int? GiangVienHuongDanId,
    string? TenGiangVienHuongDan);

public record PhanCongDeTaiDto(int DeTaiId, int HoiDongBaoVeId);
