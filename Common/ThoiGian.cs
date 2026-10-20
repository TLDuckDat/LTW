namespace StudentManagement.Common;

/// <summary>
/// Quy ước thời gian của hệ thống, không phụ thuộc múi giờ của máy chủ:
/// - Thời điểm (DateTime) luôn lưu và so sánh theo UTC; API trả về kèm "Z".
/// - Ngày (DateOnly) như "hôm nay" tính theo giờ Việt Nam (UTC+7, không có giờ mùa hè).
/// </summary>
public static class ThoiGian
{
    public static readonly TimeSpan GioVietNam = TimeSpan.FromHours(7);

    /// <summary>Ngày hiện tại theo giờ Việt Nam.</summary>
    public static DateOnly HomNay() => DateOnly.FromDateTime(DateTime.UtcNow + GioVietNam);

    /// <summary>
    /// Chuẩn hóa thời điểm client gửi lên về UTC. Chuỗi có "Z" hoặc độ lệch (+07:00) được đổi chính xác;
    /// chuỗi không kèm múi giờ được hiểu là giờ Việt Nam.
    /// </summary>
    public static DateTime VeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value - GioVietNam, DateTimeKind.Utc)
    };
}
