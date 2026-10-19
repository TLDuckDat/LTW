using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Auth.Dtos;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Auth.Services;

public class AuthService(AppDbContext db, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger) : IAuthService
{
    private const string AuthVersionClaim = "auth_ver";
    private readonly AppDbContext _db = db;
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? diaChiIp, string? userAgent)
    {
        var username = request.TenDangNhap?.Trim() ?? string.Empty;
        var account = string.IsNullOrEmpty(username)
            ? null
            : await _db.Set<TaiKhoan>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenDangNhap == username);
        var loginSucceeded = account is not null && !account.DaKhoa &&
            VerifyPassword(request.MatKhau, account.MatKhauHash);

        _db.Add(new NhatKyHoatDong
        {
            TaiKhoanId = account?.Id,
            TenDangNhap = Limit(username, 50) ?? string.Empty,
            HanhDong = "Login",
            ThanhCong = loginSucceeded,
            DiaChiIp = Limit(diaChiIp, 45),
            UserAgent = Limit(userAgent, 512)
        });
        await _db.SaveChangesAsync();

        if (!loginSucceeded)
        {
            _logger.LogWarning("Login failed for {Username} from {IpAddress}", username, diaChiIp);
            return null;
        }

        _logger.LogInformation("Login succeeded for account {AccountId} from {IpAddress}", account!.Id, diaChiIp);

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_jwt.ExpiryMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.UniqueName, account.TenDangNhap),
            new Claim(JwtRegisteredClaimNames.Name, account.HoTen),
            new Claim("role", account.VaiTro.ToString()),
            new Claim(AuthVersionClaim, GetAuthVersion(account))
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            account.Id,
            account.TenDangNhap,
            account.HoTen,
            account.VaiTro.ToString());
    }

    public async Task<bool> ChangePasswordAsync(int taiKhoanId, ChangePasswordRequest request)
    {
        if (!HasValidBcryptPassword(request.MatKhauCu) || !HasValidBcryptPassword(request.MatKhauMoi))
            throw new ArgumentException("Mật khẩu không được vượt quá 72 byte UTF-8.");

        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == taiKhoanId);
        if (account is null || account.DaKhoa || !VerifyPassword(request.MatKhauCu, account.MatKhauHash))
            return false;

        if (VerifyPassword(request.MatKhauMoi, account.MatKhauHash))
            throw new ArgumentException("Mật khẩu mới phải khác mật khẩu hiện tại.");

        account.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<AccountDto>> GetAccountsAsync(PagedQuery query)
    {
        var accounts = _db.Set<TaiKhoan>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            accounts = accounts.Where(x => x.TenDangNhap.Contains(search) || x.HoTen.Contains(search));
        }

        var total = await accounts.CountAsync();
        var items = await accounts.OrderBy(x => x.TenDangNhap)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new AccountDto(
                x.Id,
                x.TenDangNhap,
                x.HoTen,
                x.VaiTro.ToString(),
                x.DaKhoa,
                x.CreatedAt,
                x.SinhVienId))
            .ToListAsync();

        return new PagedResult<AccountDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResult<ActivityLogDto>> GetActivityLogsAsync(PagedQuery query)
    {
        var logs = _db.Set<NhatKyHoatDong>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            logs = logs.Where(x => x.TenDangNhap.Contains(search) || x.HanhDong.Contains(search));
        }

        var total = await logs.CountAsync();
        var items = await logs.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ActivityLogDto(
                x.Id,
                x.TaiKhoanId,
                x.TenDangNhap,
                x.HanhDong,
                x.ThanhCong,
                x.DiaChiIp,
                x.UserAgent,
                x.CreatedAt))
            .ToListAsync();

        return new PagedResult<ActivityLogDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<AccountDto?> ToggleLockAsync(int id)
    {
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            return null;

        if (!account.DaKhoa && account.VaiTro == VaiTro.Admin)
        {
            var anotherActiveAdminExists = await _db.Set<TaiKhoan>()
                .AnyAsync(x => x.Id != id && x.VaiTro == VaiTro.Admin && !x.DaKhoa);
            if (!anotherActiveAdminExists)
                throw new InvalidOperationException("Không thể khóa Admin đang hoạt động cuối cùng.");
        }

        account.DaKhoa = !account.DaKhoa;
        await _db.SaveChangesAsync();

        return ToAccountDto(account);
    }

    public async Task<AccountDto?> LinkStudentAsync(int id, LinkStudentRequest request)
    {
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            return null;

        if (request.SinhVienId is { } sinhVienId)
        {
            if (account.VaiTro != VaiTro.SinhVien)
                throw new InvalidOperationException("Chỉ liên kết sinh viên cho tài khoản có vai trò SinhVien.");
            if (!await _db.Set<SinhVien>().AnyAsync(x => x.Id == sinhVienId))
                throw new InvalidOperationException("Sinh viên không tồn tại.");
            if (await _db.Set<TaiKhoan>().AnyAsync(x => x.SinhVienId == sinhVienId && x.Id != id))
                throw new InvalidOperationException("Sinh viên đã được liên kết với tài khoản khác.");
        }

        account.SinhVienId = request.SinhVienId;
        await _db.SaveChangesAsync();
        return ToAccountDto(account);
    }

    private static bool HasValidBcryptPassword(string password) =>
        !string.IsNullOrEmpty(password) && Encoding.UTF8.GetByteCount(password) <= 72;

    private static string? Limit(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? null : value[..Math.Min(value.Length, maxLength)];

    private static bool VerifyPassword(string password, string? hash)
    {
        if (!HasValidBcryptPassword(password) || !IsBcryptHash(hash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private static bool IsBcryptHash(string? hash) =>
        hash is { Length: 60 } &&
        (hash.StartsWith("$2a$", StringComparison.Ordinal) ||
         hash.StartsWith("$2b$", StringComparison.Ordinal) ||
         hash.StartsWith("$2y$", StringComparison.Ordinal)) &&
        hash[4] is >= '0' and <= '9' &&
        hash[5] is >= '0' and <= '9' &&
        hash[6] == '$' &&
        hash.AsSpan(7).IndexOfAnyExcept("./ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789") < 0;

    private static string GetAuthVersion(TaiKhoan account) =>
        (account.UpdatedAt ?? account.CreatedAt).Ticks.ToString(CultureInfo.InvariantCulture);

    private static AccountDto ToAccountDto(TaiKhoan account) => new(
        account.Id,
        account.TenDangNhap,
        account.HoTen,
        account.VaiTro.ToString(),
        account.DaKhoa,
        account.CreatedAt,
        account.SinhVienId);
}
