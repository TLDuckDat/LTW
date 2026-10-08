using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic;

public static class AcademicModule
{
    /// <summary>Đăng ký service của module Academic.</summary>
    public static IServiceCollection AddAcademicModule(this IServiceCollection services)
    {
        services.AddScoped<IKhoaService, KhoaService>(); // Mẫu CRUD
        services.AddScoped<IAcademicService, AcademicService>();
        return services;
    }
}
