using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Graduation.Dtos;

public record DeTaiRequest([Required, MaxLength(500)] string TenDeTai);

public record DangKyDeTaiRequest([Range(1, int.MaxValue)] int SinhVienId);

public record PhanCongHuongDanRequest([Range(1, int.MaxValue)] int GiangVienHuongDanId);

public record DeTaiDto(int Id, string TenDeTai, int? SinhVienId, string? MaSinhVien,
    string? HoTenSinhVien, int? GiangVienHuongDanId, string? GiangVienHuongDan,
    DateTime CreatedAt, DateTime? UpdatedAt);
