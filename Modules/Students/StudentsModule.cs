using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students;

public static class StudentsModule
{
    /// <summary>Đăng ký service của module Students.</summary>
    public static IServiceCollection AddStudentsModule(this IServiceCollection services)
    {
        services.AddScoped<ISinhVienService, SinhVienService>();
        return services;
    }
}
