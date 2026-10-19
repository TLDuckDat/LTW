using StudentManagement.Modules.Registration.Services;

namespace StudentManagement.Modules.Registration;

public static class RegistrationModule
{
    /// <summary>Đăng ký service của module Registration.</summary>
    public static IServiceCollection AddRegistrationModule(this IServiceCollection services)
    {
        services.AddScoped<IDotDangKyService, DotDangKyService>();
        services.AddScoped<IDieuKienDangKyService, DieuKienDangKyService>();
        services.AddScoped<IDangKyHocPhanService, DangKyHocPhanService>();
        return services;
    }
}
