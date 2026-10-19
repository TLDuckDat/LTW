using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation;

public static class GraduationModule
{
    /// <summary>Đăng ký service của module Graduation.</summary>
    public static IServiceCollection AddGraduationModule(this IServiceCollection services)
    {
        services.AddScoped<IGraduationService, GraduationService>();
        services.AddScoped<ICoSoThucTapService, CoSoThucTapService>();
        services.AddScoped<IThucTapService, ThucTapService>();
        services.AddScoped<IDeTaiService, DeTaiService>();
        services.AddScoped<IHoiDongService, HoiDongService>();
        services.AddScoped<IKetQuaBaoVeService, KetQuaBaoVeService>();
        services.AddScoped<IDieuKienTotNghiepService, DieuKienTotNghiepService>();
        return services;
    }
}
