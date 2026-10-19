using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Dtos;
using StudentManagement.Modules.Auth.Services;

namespace StudentManagement.Modules.Auth.Controllers;

[ApiController]
[Authorize]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController(IAuthService service) : ControllerBase
{
    private readonly IAuthService _service = service;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();
        var result = await _service.LoginAsync(request, ipAddress, userAgent);
        return result is null
            ? Unauthorized(ApiResponse<LoginResponse>.Fail("Tên đăng nhập hoặc mật khẩu không chính xác."))
            : Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!TryGetCurrentAccountId(out var taiKhoanId))
            return Unauthorized(ApiResponse<object>.Fail("Token không hợp lệ."));

        try
        {
            return await _service.ChangePasswordAsync(taiKhoanId, request)
                ? Ok(ApiResponse<object>.Ok(null, "Đổi mật khẩu thành công. Vui lòng đăng nhập lại."))
                : Unauthorized(ApiResponse<object>.Fail("Mật khẩu hiện tại không chính xác."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("accounts")]
    public async Task<ActionResult<ApiResponse<PagedResult<AccountDto>>>> GetAccounts([FromQuery] PagedQuery query)
        => Ok(ApiResponse<PagedResult<AccountDto>>.Ok(await _service.GetAccountsAsync(query)));

    [Authorize(Roles = "Admin")]
    [HttpGet("activity-logs")]
    public async Task<ActionResult<ApiResponse<PagedResult<ActivityLogDto>>>> GetActivityLogs([FromQuery] PagedQuery query)
        => Ok(ApiResponse<PagedResult<ActivityLogDto>>.Ok(await _service.GetActivityLogsAsync(query)));

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/lock")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> ToggleLock(int id)
    {
        if (TryGetCurrentAccountId(out var currentId) && currentId == id)
            return Conflict(ApiResponse<AccountDto>.Fail("Không thể khóa chính tài khoản đang đăng nhập."));

        try
        {
            var result = await _service.ToggleLockAsync(id);
            return result is null
                ? NotFound(ApiResponse<AccountDto>.Fail("Không tìm thấy tài khoản."))
                : Ok(ApiResponse<AccountDto>.Ok(result, result.DaKhoa ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AccountDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/sinh-vien")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> LinkStudent(int id, [FromBody] LinkStudentRequest request)
    {
        try
        {
            var result = await _service.LinkStudentAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<AccountDto>.Fail("Không tìm thấy tài khoản."))
                : Ok(ApiResponse<AccountDto>.Ok(result, "Đã cập nhật liên kết sinh viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AccountDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/giang-vien")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> LinkLecturer(int id, [FromBody] LinkLecturerRequest request)
    {
        try
        {
            var result = await _service.LinkLecturerAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<AccountDto>.Fail("Không tìm thấy tài khoản."))
                : Ok(ApiResponse<AccountDto>.Ok(result, "Đã cập nhật liên kết giảng viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AccountDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("accounts")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            var result = await _service.CreateAccountAsync(request);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<AccountDto>.Ok(result, "Đã tạo tài khoản."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AccountDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AccountDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/reset-password")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            var result = await _service.ResetPasswordAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<AccountDto>.Fail("Không tìm thấy tài khoản."))
                : Ok(ApiResponse<AccountDto>.Ok(result, "Đã đặt lại mật khẩu. Tài khoản cần đăng nhập lại."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AccountDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("accounts/{id:int}/vai-tro")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        if (TryGetCurrentAccountId(out var currentId) && currentId == id)
            return Conflict(ApiResponse<AccountDto>.Fail("Không thể tự đổi vai trò của tài khoản đang đăng nhập."));

        try
        {
            var result = await _service.ChangeRoleAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<AccountDto>.Fail("Không tìm thấy tài khoản."))
                : Ok(ApiResponse<AccountDto>.Ok(result, "Đã đổi vai trò tài khoản."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AccountDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<AccountDto>.Fail(ex.Message));
        }
    }

    private bool TryGetCurrentAccountId(out int taiKhoanId) =>
        int.TryParse(User.FindFirst("sub")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out taiKhoanId);
}
