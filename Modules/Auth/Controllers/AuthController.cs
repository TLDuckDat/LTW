using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Dtos;
using StudentManagement.Modules.Auth.Services;

namespace StudentManagement.Modules.Auth.Controllers;

[Authorize]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController(IAuthService service, ICurrentUserService currentUser) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request) =>
        Execute(() => service.LoginAsync(request, DiaChiIp, UserAgent));

    // Đổi refresh token lấy cặp token mới; refresh token cũ hết hiệu lực ngay (chỉ dùng một lần)
    [AllowAnonymous]
    [HttpPost("refresh")]
    public Task<ActionResult<ApiResponse<LoginResponse>>> Refresh([FromBody] RefreshTokenRequest request) =>
        Execute(() => service.RefreshAsync(request.RefreshToken, DiaChiIp, UserAgent));

    // Không cần access token còn hạn để đăng xuất được cả khi token đã hết hạn
    [AllowAnonymous]
    [HttpPost("logout")]
    public Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenRequest request) =>
        Execute(() => service.LogoutAsync(request.RefreshToken, DiaChiIp, UserAgent), "Đã đăng xuất.");

    [HttpPost("logout-all")]
    public Task<ActionResult<ApiResponse<object>>> LogoutAll() =>
        Execute(() => service.LogoutAllAsync(currentUser.AccountId), "Đã đăng xuất khỏi mọi thiết bị.");

    [HttpPost("change-password")]
    public Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request) =>
        Execute(() => service.ChangePasswordAsync(currentUser.AccountId, request),
            "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.");

    // Thông tin tài khoản đang đăng nhập (vai trò, hồ sơ sinh viên/giảng viên đã liên kết)
    [HttpGet("me")]
    public Task<ActionResult<ApiResponse<AccountDto>>> Me() =>
        Execute(() => service.GetAccountAsync(currentUser.AccountId));

    [Authorize(Roles = "Admin")]
    [HttpGet("accounts")]
    public Task<ActionResult<ApiResponse<PagedResult<AccountDto>>>> GetAccounts([FromQuery] PagedQuery query) =>
        Execute(() => service.GetAccountsAsync(query));

    [Authorize(Roles = "Admin")]
    [HttpGet("activity-logs")]
    public Task<ActionResult<ApiResponse<PagedResult<ActivityLogDto>>>> GetActivityLogs([FromQuery] ActivityLogQuery query) =>
        Execute(() => service.GetActivityLogsAsync(query));

    [Authorize(Roles = "Admin")]
    [HttpGet("accounts/{id:int}")]
    public Task<ActionResult<ApiResponse<AccountDto>>> GetAccount(int id) =>
        Execute(() => service.GetAccountAsync(id));

    // Khóa/mở khóa là hai thao tác riêng, gọi lại nhiều lần vẫn cho cùng kết quả
    [Authorize(Roles = "Admin")]
    [HttpPost("accounts/{id:int}/lock")]
    public Task<ActionResult<ApiResponse<AccountDto>>> Lock(int id) =>
        Execute(() =>
            {
                KhongPhaiChinhMinh(id, "Không thể khóa chính tài khoản đang đăng nhập.");
                return service.SetLockAsync(id, khoa: true);
            },
            message: "Đã khóa tài khoản.");

    [Authorize(Roles = "Admin")]
    [HttpPost("accounts/{id:int}/unlock")]
    public Task<ActionResult<ApiResponse<AccountDto>>> Unlock(int id) =>
        Execute(() => service.SetLockAsync(id, khoa: false), message: "Đã mở khóa tài khoản.");

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/student")]
    public Task<ActionResult<ApiResponse<AccountDto>>> LinkStudent(int id, [FromBody] LinkStudentRequest request) =>
        Execute(() => service.LinkStudentAsync(id, request), message: "Đã cập nhật liên kết sinh viên.");

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/lecturer")]
    public Task<ActionResult<ApiResponse<AccountDto>>> LinkLecturer(int id, [FromBody] LinkLecturerRequest request) =>
        Execute(() => service.LinkLecturerAsync(id, request), message: "Đã cập nhật liên kết giảng viên.");

    [Authorize(Roles = "Admin")]
    [HttpPost("accounts")]
    public Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount([FromBody] CreateAccountRequest request) =>
        ExecuteCreated(() => service.CreateAccountAsync(request), x => x.Id, "Đã tạo tài khoản.");

    [Authorize(Roles = "Admin")]
    [HttpPost("accounts/{id:int}/reset-password")]
    public Task<ActionResult<ApiResponse<AccountDto>>> ResetPassword(int id, [FromBody] ResetPasswordRequest request) =>
        Execute(() => service.ResetPasswordAsync(id, request),
            message: "Đã đặt lại mật khẩu. Tài khoản cần đăng nhập lại.");

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/role")]
    public Task<ActionResult<ApiResponse<AccountDto>>> ChangeRole(int id, [FromBody] ChangeRoleRequest request) =>
        Execute(() =>
        {
            KhongPhaiChinhMinh(id, "Không thể tự đổi vai trò của tài khoản đang đăng nhập.");
            return service.ChangeRoleAsync(id, request);
        }, message: "Đã đổi vai trò tài khoản.");

    private string? DiaChiIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string UserAgent => Request.Headers.UserAgent.ToString();

    // Admin không được tự khóa/đổi vai trò của chính mình (tránh tự khóa mình khỏi hệ thống)
    private void KhongPhaiChinhMinh(int id, string message)
    {
        if (currentUser.AccountId == id)
            throw AppException.Conflict(message);
    }
}
