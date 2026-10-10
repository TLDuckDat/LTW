using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.Common;
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
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Dbcontext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Bind and validate JWT settings. A development key is generated per run;
// production must provide Jwt__Secret through a secret store or environment variable.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
          ?? throw new InvalidOperationException("Thiếu cấu hình 'Jwt' trong appsettings.json.");
if (string.IsNullOrWhiteSpace(jwt.Secret))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Hãy cấu hình Jwt:Secret bằng secret store hoặc biến môi trường Jwt__Secret.");

    jwt.Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

if (Encoding.UTF8.GetByteCount(jwt.Secret) < 32)
    throw new InvalidOperationException("Jwt:Secret phải có ít nhất 32 byte UTF-8.");
if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
    throw new InvalidOperationException("Jwt:Issuer và Jwt:Audience không được để trống.");
if (jwt.ExpiryMinutes <= 0)
    throw new InvalidOperationException("Jwt:ExpiryMinutes phải lớn hơn 0.");

builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = jwt.Secret;
    options.Issuer = jwt.Issuer;
    options.Audience = jwt.Audience;
    options.ExpiryMinutes = jwt.ExpiryMinutes;
});

// 3. Authentication + Authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                if (!int.TryParse(principal?.FindFirst("sub")?.Value, out var accountId))
                {
                    context.Fail("Token không hợp lệ.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var account = await db.Set<StudentManagement.Modules.Auth.Entities.TaiKhoan>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == accountId, context.HttpContext.RequestAborted);
                var tokenVersion = principal?.FindFirst("auth_ver")?.Value;
                var currentVersion = account is null
                    ? null
                    : (account.UpdatedAt ?? account.CreatedAt).Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);

                if (account is null || account.DaKhoa ||
                    principal?.FindFirst("role")?.Value != account.VaiTro.ToString() ||
                    tokenVersion != currentVersion)
                {
                    context.Fail("Tài khoản không còn hoạt động hoặc token đã bị thu hồi.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// 4. Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Swagger
builder.Services.AddSwaggerGen();

// 6. Đăng ký các Modules
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
