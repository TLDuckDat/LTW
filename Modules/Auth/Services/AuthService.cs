using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Auth.Dtos;

namespace StudentManagement.Modules.Auth.Services;

public class AuthService(AppDbContext db) : IAuthService
{
    private readonly AppDbContext _db = db;

    // AUTH-01 Đăng nhập, cấp JWT
    public Task<object?> LoginAsync(LoginRequest request) => throw new NotImplementedException("TODO");

    // AUTH-02 Đổi mật khẩu
    public Task<object?> ChangePasswordAsync(ChangePasswordRequest request) => throw new NotImplementedException("TODO");

    // AUTH-03 Danh sách tài khoản
    public Task<object?> GetAccountsAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // AUTH-04 Khóa / mở khóa tài khoản
    public Task<object?> ToggleLockAsync(int id) => throw new NotImplementedException("TODO");
}
