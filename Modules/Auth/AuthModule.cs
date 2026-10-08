using StudentManagement.Modules.Auth.Services;

namespace StudentManagement.Modules.Auth;

public static class AuthModule
{
    /// <summary>Đăng ký service của module Auth.</summary>
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
