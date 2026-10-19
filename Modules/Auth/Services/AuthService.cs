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
                x.SinhVienId,
                x.GiangVienId))
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

        if (!account.DaKhoa && account.VaiTro == VaiTro.Admin && !await AnotherActiveAdminExistsAsync(id))
            throw new InvalidOperationException("Không thể khóa Admin đang hoạt động cuối cùng.");

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
            await EnsureCanLinkStudentAsync(id, account.VaiTro, sinhVienId);

        account.SinhVienId = request.SinhVienId;
        await _db.SaveChangesAsync();
        return ToAccountDto(account);
    }

    public async Task<AccountDto?> LinkLecturerAsync(int id, LinkLecturerRequest request)
    {
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            return null;

        if (request.GiangVienId is { } giangVienId)
            await EnsureCanLinkLecturerAsync(id, account.VaiTro, giangVienId);

        account.GiangVienId = request.GiangVienId;
        await _db.SaveChangesAsync();
        return ToAccountDto(account);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
    {
        var username = request.TenDangNhap.Trim();
        var vaiTro = request.VaiTro ?? throw new ArgumentException("Vai trò là bắt buộc.");
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Tên đăng nhập không được để trống.");
        if (!HasValidBcryptPassword(request.MatKhau))
            throw new ArgumentException("Mật khẩu không được vượt quá 72 byte UTF-8.");
        if (await _db.Set<TaiKhoan>().AnyAsync(x => x.TenDangNhap == username))
            throw new InvalidOperationException($"Tên đăng nhập '{username}' đã tồn tại.");
        if (request.SinhVienId is { } sinhVienId)
            await EnsureCanLinkStudentAsync(null, vaiTro, sinhVienId);
        if (request.GiangVienId is { } giangVienId)
            await EnsureCanLinkLecturerAsync(null, vaiTro, giangVienId);

        var account = new TaiKhoan
        {
            TenDangNhap = username,
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
            HoTen = request.HoTen.Trim(),
            VaiTro = vaiTro,
            SinhVienId = request.SinhVienId,
            GiangVienId = request.GiangVienId
        };
        _db.Add(account);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw new InvalidOperationException("Tên đăng nhập hoặc hồ sơ liên kết đã được dùng cho tài khoản khác.");
        }

        _logger.LogInformation("Account {AccountId} created with role {Role}", account.Id, account.VaiTro);
        return ToAccountDto(account);
    }

    // Đổi mật khẩu làm thay đổi UpdatedAt nên các token cũ của tài khoản bị thu hồi
    public async Task<AccountDto?> ResetPasswordAsync(int id, ResetPasswordRequest request)
    {
        if (!HasValidBcryptPassword(request.MatKhauMoi))
            throw new ArgumentException("Mật khẩu không được vượt quá 72 byte UTF-8.");

        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            return null;

        account.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Password of account {AccountId} reset by admin", id);
        return ToAccountDto(account);
    }

    public async Task<AccountDto?> ChangeRoleAsync(int id, ChangeRoleRequest request)
    {
        var vaiTro = request.VaiTro ?? throw new ArgumentException("Vai trò là bắt buộc.");
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            return null;
        if (account.VaiTro == vaiTro)
            return ToAccountDto(account);

        if (account.VaiTro == VaiTro.Admin && !account.DaKhoa && !await AnotherActiveAdminExistsAsync(id))
            throw new InvalidOperationException("Không thể đổi vai trò của Admin đang hoạt động cuối cùng.");

        // Liên kết hồ sơ chỉ có nghĩa với đúng vai trò, nên gỡ khi đổi sang vai trò khác
        account.VaiTro = vaiTro;
        if (vaiTro != VaiTro.SinhVien) account.SinhVienId = null;
        if (vaiTro != VaiTro.GiangVien) account.GiangVienId = null;
        await _db.SaveChangesAsync();
        return ToAccountDto(account);
    }

    private Task<bool> AnotherActiveAdminExistsAsync(int id) =>
        _db.Set<TaiKhoan>().AnyAsync(x => x.Id != id && x.VaiTro == VaiTro.Admin && !x.DaKhoa);

    private async Task EnsureCanLinkStudentAsync(int? accountId, VaiTro vaiTro, int sinhVienId)
    {
        if (vaiTro != VaiTro.SinhVien)
            throw new InvalidOperationException("Chỉ liên kết sinh viên cho tài khoản có vai trò SinhVien.");
        if (!await _db.Set<SinhVien>().AnyAsync(x => x.Id == sinhVienId))
            throw new InvalidOperationException("Sinh viên không tồn tại.");
        if (await _db.Set<TaiKhoan>().AnyAsync(x => x.SinhVienId == sinhVienId && x.Id != accountId))
            throw new InvalidOperationException("Sinh viên đã được liên kết với tài khoản khác.");
    }

    private async Task EnsureCanLinkLecturerAsync(int? accountId, VaiTro vaiTro, int giangVienId)
    {
        if (vaiTro != VaiTro.GiangVien)
            throw new InvalidOperationException("Chỉ liên kết giảng viên cho tài khoản có vai trò GiangVien.");
        if (!await _db.Set<GiangVien>().AnyAsync(x => x.Id == giangVienId))
            throw new InvalidOperationException("Giảng viên không tồn tại.");
        if (await _db.Set<TaiKhoan>().AnyAsync(x => x.GiangVienId == giangVienId && x.Id != accountId))
            throw new InvalidOperationException("Giảng viên đã được liên kết với tài khoản khác.");
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
        account.SinhVienId,
        account.GiangVienId);
}
