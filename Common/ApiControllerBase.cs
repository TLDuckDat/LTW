using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.Common;

/// <summary>
/// Lớp cha cho controller. Mỗi action chỉ cần gọi <c>Execute(() => service.XxxAsync(...))</c>:
/// kết quả được bọc trong <see cref="ApiResponse{T}"/>, còn <see cref="AppException"/> thành phản hồi lỗi.
///
/// Quy ước kiểm tra dữ liệu:
/// - Ràng buộc định dạng (bắt buộc, độ dài, khoảng giá trị) khai báo bằng attribute trên DTO;
///   <see cref="ValidateRequestAttribute"/> kiểm tra trước khi vào action.
/// - Service chỉ kiểm tra quy tắc nghiệp vụ (tồn tại, trạng thái...) và ném <see cref="AppException"/>.
/// </summary>
[ApiController]
[ValidateRequest]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Chạy action có dữ liệu trả về, ví dụ lấy chi tiết hoặc tạo mới.</summary>
    protected async Task<ActionResult<ApiResponse<T>>> Execute<T>(
        Func<Task<T>> action, int successStatus = StatusCodes.Status200OK, string? message = null)
    {
        try
        {
            var result = await action();
            return StatusCode(successStatus, ApiResponse<T>.Ok(result, message));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<T>.Fail(ex.Message));
        }
    }

    /// <summary>Chạy action không có dữ liệu trả về, ví dụ hủy hoặc xóa; chỉ trả thông báo.</summary>
    protected async Task<ActionResult<ApiResponse<object>>> Execute(Func<Task> action, string message)
    {
        try
        {
            await action();
            return Ok(ApiResponse<object>.Ok(null, message));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.Message));
        }
    }
}
