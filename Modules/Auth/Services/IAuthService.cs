using StudentManagement.Common;
using StudentManagement.Modules.Auth.Dtos;

namespace StudentManagement.Modules.Auth.Services;

/// <summary>Nghiệp vụ module Auth (Epic AUTH).</summary>
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? diaChiIp, string? userAgent);
    Task<LoginResponse> RefreshAsync(string refreshToken, string? diaChiIp, string? userAgent);
    Task LogoutAsync(string refreshToken, string? diaChiIp, string? userAgent);
    Task LogoutAllAsync(int taiKhoanId);
    Task ChangePasswordAsync(int taiKhoanId, ChangePasswordRequest request);
    Task<PagedResult<AccountDto>> GetAccountsAsync(PagedQuery query);
    Task<AccountDto> GetAccountAsync(int id);
    Task<PagedResult<ActivityLogDto>> GetActivityLogsAsync(ActivityLogQuery query);
    Task<AccountDto> SetLockAsync(int id, bool khoa);
    Task<AccountDto> LinkStudentAsync(int id, LinkStudentRequest request);
    Task<AccountDto> LinkLecturerAsync(int id, LinkLecturerRequest request);
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request);
    Task<AccountDto> ResetPasswordAsync(int id, ResetPasswordRequest request);
    Task<AccountDto> ChangeRoleAsync(int id, ChangeRoleRequest request);
}
