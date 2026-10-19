using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudentManagement.Common;

/// <summary>
/// Khi dữ liệu gửi lên sai ràng buộc khai báo trên DTO ([Required], [Range], [MaxLength]...),
/// trả về 400 theo định dạng <see cref="ApiResponse{T}"/> thay vì định dạng mặc định của ASP.NET.
/// </summary>
public class ValidateRequestAttribute : ActionFilterAttribute
{
    // Chạy trước bộ lọc mặc định của [ApiController] (Order = -2000) để dùng định dạng của dự án.
    public ValidateRequestAttribute() => Order = -3000;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var fields = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => x.Key);
        var message = $"Dữ liệu đầu vào không hợp lệ: {string.Join(", ", fields)}.";
        context.Result = new BadRequestObjectResult(ApiResponse<string>.Fail(message));
    }
}
