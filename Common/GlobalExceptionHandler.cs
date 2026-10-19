using Microsoft.AspNetCore.Diagnostics;

namespace StudentManagement.Common;

/// <summary>
/// Lưới an toàn cho mọi exception chưa được xử lý, để client luôn nhận về định dạng <see cref="ApiResponse{T}"/>:
/// - <see cref="AppException"/> (ném ngoài <see cref="ApiControllerBase.Execute{T}"/>, ví dụ trong filter) giữ nguyên mã lỗi.
/// - Lỗi khác là lỗi kỹ thuật: ghi log đầy đủ, trả 500 với thông báo chung (chỉ kèm chi tiết ở Development).
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted) return false;

        int status;
        string message;
        if (exception is AppException appException)
        {
            status = appException.StatusCode;
            message = appException.Message;
        }
        else if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            // Client đã ngắt kết nối: không có ai nhận phản hồi, không coi là lỗi hệ thống
            context.Response.StatusCode = 499;
            return true;
        }
        else
        {
            logger.LogError(exception, "Lỗi chưa xử lý khi gọi {Method} {Path}", context.Request.Method, context.Request.Path);
            status = StatusCodes.Status500InternalServerError;
            message = environment.IsDevelopment()
                ? $"Đã xảy ra lỗi hệ thống: {exception.GetType().Name}: {exception.Message}"
                : "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(message), cancellationToken);
        return true;
    }
}
