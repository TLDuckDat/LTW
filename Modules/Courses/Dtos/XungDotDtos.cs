using System.Text.Json.Serialization;

namespace StudentManagement.Modules.Courses.Dtos;

/// <summary>Loại xung đột lịch học.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoaiXungDot
{
    /// <summary>Trùng phòng học cùng thời điểm với lớp khác.</summary>
    Phong,
    /// <summary>Giảng viên có lớp khác cùng thời điểm.</summary>
    GiangVien,
    /// <summary>Trùng với buổi học khác của chính lớp này.</summary>
    Lop
}

/// <summary>Một xung đột với buổi học đã có.</summary>
public record XungDotDto(
    LoaiXungDot Loai,
    int LichHocId,
    int LopHocPhanId,
    string MaLopHocPhan,
    int Thu,
    int TietBatDau,
    int SoTiet,
    string ThongBao);

/// <summary>Kết quả kiểm tra xung đột (không lưu dữ liệu).</summary>
public record KiemTraXungDotDto(bool CoXungDot, IReadOnlyList<XungDotDto> XungDot);
