using StudentManagement.Common;
using StudentManagement.Modules.Grades.Entities;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>
/// Quy chế tính điểm dùng chung cho cả module Grades. Các hàm ở đây chỉ tính toán, không đọc DB,
/// nên muốn đổi quy chế (trọng số, thang điểm chữ, ngưỡng đạt...) chỉ cần sửa trong file này.
/// </summary>
public static class GradeCalculator
{
    // Trọng số điểm thành phần trong điểm tổng kết.
    public const decimal TrongSoQuaTrinh = 0.15m;
    public const decimal TrongSoGiuaKy = 0.15m;
    public const decimal TrongSoCuoiKy = 0.70m;

    /// <summary>Điểm tổng kết từ mức này trở lên là đạt học phần (tương ứng điểm chữ D).</summary>
    public const decimal DiemDat = 4.0m;

    /// <summary>Điểm tổng kết hệ 10, làm tròn 1 chữ số thập phân (0.05 làm tròn lên).</summary>
    public static decimal TinhDiemTongKet(decimal quaTrinh, decimal giuaKy, decimal cuoiKy) =>
        decimal.Round(
            quaTrinh * TrongSoQuaTrinh + giuaKy * TrongSoGiuaKy + cuoiKy * TrongSoCuoiKy,
            1, MidpointRounding.AwayFromZero);

    public static bool LaDiemDat(decimal diemTongKet) => diemTongKet >= DiemDat;

    public static string QuyDoiDiemChu(decimal diemTongKet) => diemTongKet switch
    {
        >= 9.5m => "A+",
        >= 8.5m => "A",
        >= 8.0m => "B+",
        >= 7.0m => "B",
        >= 6.0m => "C+",
        >= 5.5m => "C",
        >= 4.5m => "D+",
        >= 4.0m => "D",
        >= 2.0m => "F+",
        _ => "F"
    };

    public static decimal QuyDoiDiemHe4(string diemChu) => diemChu switch
    {
        "A+" => 4.0m,
        "A" => 3.8m,
        "B+" => 3.5m,
        "B" => 3.0m,
        "C+" => 2.5m,
        "C" => 2.0m,
        "D+" => 1.5m,
        "D" => 1.0m,
        "F+" => 0.5m,
        "F" => 0.0m,
        _ => throw AppException.Conflict($"Điểm chữ '{diemChu}' trong dữ liệu không hợp lệ.")
    };

    /// <summary>Thang xếp loại học lực theo điểm hệ 4 (GPA học kỳ hoặc CPA), xét từ ngưỡng cao xuống.</summary>
    public static readonly IReadOnlyList<(string Ma, string Ten, decimal NguongDuoi)> ThangXepLoai =
    [
        ("XuatSac", "Xuất sắc", 3.6m),
        ("Gioi", "Giỏi", 3.2m),
        ("Kha", "Khá", 2.5m),
        ("TrungBinh", "Trung bình", 2.0m),
        ("Yeu", "Yếu", 0m)
    ];

    public static (string Ma, string Ten, decimal NguongDuoi) XepLoai(decimal diemHe4) =>
        ThangXepLoai.First(x => diemHe4 >= x.NguongDuoi);

    /// <summary>Điểm trung bình có trọng số tín chỉ (GPA/CPA), làm tròn 2 chữ số; null nếu chưa có tín chỉ nào.</summary>
    public static decimal? TinhTrungBinh(decimal tongDiem, int soTinChi) => soTinChi == 0
        ? null
        : decimal.Round(tongDiem / soTinChi, 2, MidpointRounding.AwayFromZero);

    /// <summary>Điểm lưu ở cột decimal(4,2): nhận 7.5 hoặc 7.25 nhưng không nhận 7.555.</summary>
    public static bool CoToiDaHaiChuSoThapPhan(decimal diem) => decimal.Round(diem, 2) == diem;

    /// <summary>Đã có đủ điểm quá trình, giữa kỳ và cuối kỳ.</summary>
    public static bool DuDiemThanhPhan(DiemHocPhan diem) =>
        diem.DiemQuaTrinh is not null && diem.DiemGiuaKy is not null && diem.DiemCuoiKy is not null;

    /// <summary>Đã tính điểm tổng kết và điểm chữ.</summary>
    public static bool DaTinhTongKet(DiemHocPhan diem) =>
        diem.DiemTongKet is not null && !string.IsNullOrWhiteSpace(diem.DiemChu);
}
