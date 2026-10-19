using StudentManagement.Common;
using StudentManagement.Modules.Auth.Dtos;

namespace StudentManagement.Modules.Auth.Services;

/// <summary>Nghiệp vụ module Auth (Epic AUTH).</summary>
public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string? diaChiIp, string? userAgent);
    Task<bool> ChangePasswordAsync(int taiKhoanId, ChangePasswordRequest request);
    Task<PagedResult<AccountDto>> GetAccountsAsync(PagedQuery query);
    Task<PagedResult<ActivityLogDto>> GetActivityLogsAsync(PagedQuery query);
    Task<AccountDto?> ToggleLockAsync(int id);
    Task<AccountDto?> LinkStudentAsync(int id, LinkStudentRequest request);
}
