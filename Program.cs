using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Modules.Academic;
using StudentManagement.Modules.Auth;
using StudentManagement.Modules.Courses;
using StudentManagement.Modules.Grades;
using StudentManagement.Modules.Graduation;
using StudentManagement.Modules.Registration;
using StudentManagement.Modules.Reports;
using StudentManagement.Modules.StudentAffairs;
using StudentManagement.Modules.Students;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký các module
builder.Services
    .AddAuthModule()
    .AddStudentsModule()
    .AddAcademicModule()
    .AddCoursesModule()
    .AddRegistrationModule()
    .AddGradesModule()
    .AddStudentAffairsModule()
    .AddGraduationModule()
    .AddReportsModule();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Tự áp dụng migration và seed dữ liệu mẫu
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
