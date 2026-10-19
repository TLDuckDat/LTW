using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs;

public static class StudentAffairsModule
{
    /// <summary>Đăng ký service của module StudentAffairs.</summary>
    public static IServiceCollection AddStudentAffairsModule(this IServiceCollection services)
    {
        services.AddScoped<IDiemRenLuyenService, DiemRenLuyenService>();
        services.AddScoped<IHocPhiService, HocPhiService>();
        services.AddScoped<IMienGiamService, MienGiamService>();
        services.AddScoped<IHocBongService, HocBongService>();
        services.AddScoped<IKhenThuongKyLuatService, KhenThuongKyLuatService>();
        return services;
    }
}
