using StudentManagement.Modules.Reports.Services;

namespace StudentManagement.Modules.Reports;

public static class ReportsModule
{
    /// <summary>Đăng ký service của module Reports.</summary>
    public static IServiceCollection AddReportsModule(this IServiceCollection services)
    {
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IThongBaoService, ThongBaoService>();
        return services;
    }
}
