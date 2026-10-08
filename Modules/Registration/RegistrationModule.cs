using StudentManagement.Modules.Registration.Services;

namespace StudentManagement.Modules.Registration;

public static class RegistrationModule
{
    /// <summary>Đăng ký service của module Registration.</summary>
    public static IServiceCollection AddRegistrationModule(this IServiceCollection services)
    {
        services.AddScoped<IRegistrationService, RegistrationService>();
        return services;
    }
}
