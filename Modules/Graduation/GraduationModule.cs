using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation;

public static class GraduationModule
{
    /// <summary>Đăng ký service của module Graduation.</summary>
    public static IServiceCollection AddGraduationModule(this IServiceCollection services)
    {
        services.AddScoped<IGraduationService, GraduationService>();
        return services;
    }
}
