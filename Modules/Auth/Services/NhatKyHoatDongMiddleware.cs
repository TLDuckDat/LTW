using System.Globalization;
using Microsoft.AspNetCore.Mvc.Controllers;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Auth.Entities;

namespace StudentManagement.Modules.Auth.Services;

/// <summary>
/// Ghi nhật ký mọi thao tác ghi dữ liệu (POST/PUT/PATCH/DELETE) của người đã đăng nhập: ai, hành động, tham số
/// đường dẫn/query, mã phản hồi (kể cả 403 bị từ chối và 500), IP, user agent và chi tiết nghiệp vụ từ
/// <see cref="NhatKyContext"/>. Không ghi body request (có thể chứa mật khẩu). Thao tác đọc (GET) không ghi.
/// </summary>
public class NhatKyHoatDongMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<NhatKyHoatDongMiddleware> logger)
{
    private static readonly HashSet<string> PhuongThucGhi = ["POST", "PUT", "PATCH", "DELETE"];

    // Các action này tự ghi nhật ký trong AuthService (có cả người chưa đăng nhập)
    private static readonly HashSet<string> TuGhiNhatKy = ["Auth.Login", "Auth.Refresh", "Auth.Logout"];

    public async Task InvokeAsync(HttpContext context, NhatKyContext nhatKy)
    {
        if (!PhuongThucGhi.Contains(context.Request.Method))
        {
            await next(context);
            return;
        }

        var maPhanHoi = StatusCodes.Status500InternalServerError;
        try
        {
            await next(context);
            maPhanHoi = context.Response.StatusCode;
        }
        finally
        {
            // Người dùng được xác thực ở middleware phía trong, nên đến đây mới biết là ai
            if (context.User.Identity?.IsAuthenticated == true)
                await GhiAsync(context, nhatKy, maPhanHoi);
        }
    }

    private async Task GhiAsync(HttpContext context, NhatKyContext nhatKy, int maPhanHoi)
    {
        var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var hanhDong = action is null ? "KhongXacDinh" : $"{action.ControllerName}.{action.ActionName}";
        if (TuGhiNhatKy.Contains(hanhDong)) return;

        try
        {
            var thamSo = context.Request.RouteValues
                .Where(x => x.Key is not ("controller" or "action"))
                .Select(x => $"{x.Key}={x.Value}");
            var chiTiet = string.Join("; ", thamSo
                .Concat(context.Request.Query.Select(x => $"{x.Key}={x.Value}"))
                .Concat(nhatKy.ChiTiet));

            // CSDL riêng: DbContext của request có thể đang giữ thay đổi lỗi, không được lưu kèm
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Add(new NhatKyHoatDong
            {
                TaiKhoanId = int.TryParse(context.User.FindFirst("sub")?.Value, NumberStyles.None,
                    CultureInfo.InvariantCulture, out var id) ? id : null,
                TenDangNhap = Cat(context.User.FindFirst("unique_name")?.Value, 50) ?? string.Empty,
                HanhDong = Cat(hanhDong, 100)!,
                ThanhCong = maPhanHoi < 400,
                PhuongThuc = context.Request.Method,
                DuongDan = Cat(context.Request.Path.Value, 300),
                MaPhanHoi = maPhanHoi,
                ChiTiet = Cat(chiTiet, 1000),
                DiaChiIp = Cat(context.Connection.RemoteIpAddress?.ToString(), 45),
                UserAgent = Cat(context.Request.Headers.UserAgent.ToString(), 512)
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Lỗi ghi nhật ký không được làm hỏng phản hồi của người dùng
            logger.LogError(ex, "Không ghi được nhật ký hoạt động cho {Method} {Path}", context.Request.Method, context.Request.Path);
        }
    }

    private static string? Cat(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? null : value[..Math.Min(value.Length, maxLength)];
}
