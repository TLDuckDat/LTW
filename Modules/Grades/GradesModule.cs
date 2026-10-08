using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades;

public static class GradesModule
{
    /// <summary>Đăng ký service của module Grades.</summary>
    public static IServiceCollection AddGradesModule(this IServiceCollection services)
    {
        services.AddScoped<IGradeService, GradeService>();
        return services;
    }
}
