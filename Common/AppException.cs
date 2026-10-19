namespace StudentManagement.Common;

/// <summary>
/// Lỗi nghiệp vụ có thể báo thẳng cho người dùng (không tìm thấy, dữ liệu sai, xung đột trạng thái...).
/// Service ném lỗi này; <see cref="ApiControllerBase"/> bắt lại và trả về đúng <see cref="StatusCode"/>.
/// Lỗi kỹ thuật (bug, mất kết nối DB) KHÔNG dùng lớp này, để chúng thành lỗi 500 và dễ phát hiện.
/// </summary>
public class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    /// <summary>400: dữ liệu gửi lên không hợp lệ.</summary>
    public static AppException BadRequest(string message) =>
        new(StatusCodes.Status400BadRequest, message);

    /// <summary>404 với thông báo "Không tìm thấy {doiTuong}.", ví dụ NotFound("đề tài").</summary>
    public static AppException NotFound(string doiTuong) =>
        new(StatusCodes.Status404NotFound, $"Không tìm thấy {doiTuong}.");

    /// <summary>409: yêu cầu hợp lệ nhưng trái với trạng thái hiện tại của dữ liệu.</summary>
    public static AppException Conflict(string message) =>
        new(StatusCodes.Status409Conflict, message);
}
