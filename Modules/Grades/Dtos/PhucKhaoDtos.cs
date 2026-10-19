using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Grades.Dtos;

/// <summary>Tạo yêu cầu phúc khảo điểm cuối kỳ.</summary>
public record PhucKhaoRequest(
    [Range(1, int.MaxValue)] int DiemHocPhanId,
    [Required, MaxLength(500)] string LyDo);

/// <summary>Quyết định phúc khảo; chỉ gửi điểm mới khi chấp nhận.</summary>
public record XuLyPhucKhaoRequest(
    [Required] bool? ChapNhan,
    [Range(0d, 10d)] decimal? DiemMoi);

/// <summary>Thông tin yêu cầu phúc khảo và sinh viên liên quan.</summary>
public record PhucKhaoDto(
    int Id,
    int DiemHocPhanId,
    int SinhVienId,
    string MSSV,
    string HoTen,
    string LyDo,
    decimal? DiemCu,
    decimal? DiemMoi,
    string TrangThai,
    DateTime NgayTao);
