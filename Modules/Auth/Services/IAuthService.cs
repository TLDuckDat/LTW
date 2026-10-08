using StudentManagement.Common;
using StudentManagement.Modules.Auth.Dtos;

namespace StudentManagement.Modules.Auth.Services;

/// <summary>Nghiệp vụ module Auth (Epic AUTH).</summary>
public interface IAuthService
{
    // AUTH-01 Đăng nhập, cấp JWT
    Task<object?> LoginAsync(LoginRequest request);
    // AUTH-02 Đổi mật khẩu
    Task<object?> ChangePasswordAsync(ChangePasswordRequest request);
    // AUTH-03 Danh sách tài khoản
    Task<object?> GetAccountsAsync(PagedQuery query);
    // AUTH-04 Khóa / mở khóa tài khoản
    Task<object?> ToggleLockAsync(int id);
}
