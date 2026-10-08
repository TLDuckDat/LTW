using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Dtos;
using StudentManagement.Modules.Auth.Services;

namespace StudentManagement.Modules.Auth.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController(IAuthService service) : ControllerBase
{
    private readonly IAuthService _service = service;

    // AUTH-01 Đăng nhập, cấp JWT
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.LoginAsync(request)));

    // AUTH-02 Đổi mật khẩu
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.ChangePasswordAsync(request)));

    // AUTH-03 Danh sách tài khoản
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetAccountsAsync(query)));

    // AUTH-04 Khóa / mở khóa tài khoản
    [HttpPut("accounts/{id:int}/lock")]
    public async Task<IActionResult> ToggleLock(int id)
        => Ok(ApiResponse<object>.Ok(await _service.ToggleLockAsync(id)));
}
