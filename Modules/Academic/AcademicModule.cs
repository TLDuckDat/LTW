using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic;

public static class AcademicModule
{
    /// <summary>Đăng ký service của module Academic (EPIC-03: EDU-01 đến EDU-06).</summary>
    public static IServiceCollection AddAcademicModule(this IServiceCollection services)
    {
        services.AddScoped<IKhoaService, KhoaService>(); // EDU-01 Mẫu CRUD Khoa
        services.AddScoped<IBoMonService, BoMonService>(); // EDU-01 Quản lý Bộ môn
        services.AddScoped<INganhService, NganhService>(); // EDU-02 Quản lý Ngành
        services.AddScoped<IChuyenNganhService, ChuyenNganhService>(); // EDU-02 Quản lý Chuyên ngành
        services.AddScoped<INamHocService, NamHocService>(); // EDU-03 Quản lý Năm học
        services.AddScoped<IHocKyService, HocKyService>(); // EDU-03 Quản lý Học kỳ
        services.AddScoped<IChuongTrinhDaoTaoService, ChuongTrinhDaoTaoService>(); // EDU-04 & EDU-05 Quản lý CTĐT và Môn học trong CTĐT
        services.AddScoped<IQuyCheHocVuService, QuyCheHocVuService>(); // EDU-06 Quản lý Quy chế học vụ
        return services;
    }
}
