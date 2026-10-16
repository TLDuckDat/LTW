using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses;

public static class CoursesModule
{
    /// <summary>Đăng ký service của module Courses.</summary>
    public static IServiceCollection AddCoursesModule(this IServiceCollection services)
    {
        services.AddScoped<IMonHocService, MonHocService>();
        services.AddScoped<ILopHocPhanService, LopHocPhanService>();
        return services;
    }
}
