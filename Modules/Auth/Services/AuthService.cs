using System.Globalization;
using System.Security.Cryptography;
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

public class AuthService(
    AppDbContext db, IOptions<JwtSettings> jwtOptions, NhatKyContext nhatKy, ILogger<AuthService> logger) : IAuthService
{
    private const string AuthVersionClaim = "auth_ver";
    private readonly AppDbContext _db = db;
    private readonly JwtSettings _jwt = jwtOptions.Value;
    private readonly NhatKyContext _nhatKy = nhatKy;
    private readonly ILogger<AuthService> _logger = logger;

    /// <summary>Số lần đăng nhập sai tối đa trong <see cref="ThoiGianTheoDoi"/>; vượt quá thì tạm chặn đăng nhập.</summary>
    public const int SoLanSaiToiDa = 5;
    public static readonly TimeSpan ThoiGianTheoDoi = TimeSpan.FromMinutes(30);

    private const string HanhDongDangNhap = "Login";
    private const string PhienKhongHopLe = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.";

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? diaChiIp, string? userAgent)
    {
        var username = Limit(request.TenDangNhap?.Trim(), 50) ?? string.Empty;
        var account = string.IsNullOrEmpty(username)
            ? null
            : await _db.Set<TaiKhoan>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenDangNhap == username);
        var bay = DateTime.UtcNow;

        // Chống dò mật khẩu: đếm theo tên đăng nhập, kể cả tên không tồn tại (không lộ tài khoản nào có thật).
        // Đã chặn thì từ chối cả khi mật khẩu đúng, cho tới khi lần sai cũ nhất trong 5 lần ra khỏi khung 30 phút.
        var lanSai = await LanDangNhapSaiGanDayAsync(username, account, bay);
        if (lanSai.Count >= SoLanSaiToiDa)
        {
            var moKhoaLuc = lanSai[^SoLanSaiToiDa] + ThoiGianTheoDoi;
            await GhiNhatKyAsync(account?.Id, username, "LoginBiChan", false, diaChiIp, userAgent);
            _logger.LogWarning("Login blocked for {Username} from {IpAddress} until {UnlockAt}", username, diaChiIp, moKhoaLuc);
            var soPhut = Math.Max(1, (int)Math.Ceiling((moKhoaLuc - bay).TotalMinutes));
            throw new AppException(StatusCodes.Status429TooManyRequests,
                $"Đăng nhập sai quá {SoLanSaiToiDa} lần trong {ThoiGianTheoDoi.TotalMinutes:0} phút. Vui lòng thử lại sau {soPhut} phút.");
        }

        var loginSucceeded = account is not null && !account.DaKhoa &&
            VerifyPassword(request.MatKhau, account.MatKhauHash);
        await GhiNhatKyAsync(account?.Id, username, HanhDongDangNhap, loginSucceeded, diaChiIp, userAgent);

        if (!loginSucceeded)
        {
            _logger.LogWarning("Login failed for {Username} from {IpAddress}", username, diaChiIp);
            var conLai = SoLanSaiToiDa - lanSai.Count - 1;
            throw AppException.Unauthorized(conLai > 0
                ? $"Tên đăng nhập hoặc mật khẩu không chính xác. Còn {conLai} lần thử."
                : $"Tên đăng nhập hoặc mật khẩu không chính xác. Đăng nhập bị tạm chặn {ThoiGianTheoDoi.TotalMinutes:0} phút.");
        }

        _logger.LogInformation("Login succeeded for account {AccountId} from {IpAddress}", account!.Id, diaChiIp);
        return await TaoPhienAsync(account, diaChiIp, userAgent);
    }

    // Đổi refresh token lấy cặp token mới (xoay vòng: token cũ bị thu hồi ngay)
    public async Task<LoginResponse> RefreshAsync(string refreshToken, string? diaChiIp, string? userAgent)
    {
        var token = await _db.Set<RefreshToken>().FirstOrDefaultAsync(x => x.TokenHash == HashToken(refreshToken))
            ?? throw AppException.Unauthorized(PhienKhongHopLe);
        var account = await _db.Set<TaiKhoan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == token.TaiKhoanId);
        var bay = DateTime.UtcNow;

        if (token.ThuHoiLuc is not null)
        {
            // Token đã đổi lấy token mới mà vẫn bị dùng lại: có thể đã bị đánh cắp, thu hồi mọi phiên của tài khoản
            if (token.LyDoThuHoi == LyDoThuHoiToken.XoayVong)
            {
                var soPhien = await ThuHoiMoiPhienAsync(token.TaiKhoanId, LyDoThuHoiToken.NghiBiDanhCap, bay);
                await GhiNhatKyAsync(token.TaiKhoanId, account?.TenDangNhap ?? string.Empty, "RefreshTaiSuDung", false,
                    diaChiIp, userAgent, $"Refresh token đã dùng bị dùng lại; thu hồi {soPhien} phiên đang hoạt động.");
                _logger.LogWarning("Refresh token reuse detected for account {AccountId} from {IpAddress}", token.TaiKhoanId, diaChiIp);
            }
            throw AppException.Unauthorized(PhienKhongHopLe);
        }
        if (token.HetHan <= bay)
            throw AppException.Unauthorized(PhienKhongHopLe);

        // Khóa, đổi mật khẩu, đổi vai trò... sau khi cấp token đều làm thay đổi UpdatedAt của tài khoản
        if (account is null || account.DaKhoa || (account.UpdatedAt ?? account.CreatedAt) > token.CreatedAt)
        {
            token.ThuHoiLuc = bay;
            token.LyDoThuHoi = LyDoThuHoiToken.TaiKhoanThayDoi;
            await _db.SaveChangesAsync();
            throw AppException.Unauthorized("Phiên đăng nhập không còn hiệu lực do tài khoản đã thay đổi. Vui lòng đăng nhập lại.");
        }

        // Cập nhật có điều kiện: hai yêu cầu làm mới cùng lúc bằng một token thì chỉ một yêu cầu thành công
        var thuHoi = await _db.Set<RefreshToken>()
            .Where(x => x.Id == token.Id && x.ThuHoiLuc == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ThuHoiLuc, bay)
                .SetProperty(x => x.LyDoThuHoi, LyDoThuHoiToken.XoayVong)
                .SetProperty(x => x.UpdatedAt, bay));
        if (thuHoi == 0)
            throw AppException.Unauthorized(PhienKhongHopLe);

        await GhiNhatKyAsync(account.Id, account.TenDangNhap, "Refresh", true, diaChiIp, userAgent);
        return await TaoPhienAsync(account, diaChiIp, userAgent);
    }

    // Đăng xuất phiên hiện tại; gọi lại nhiều lần vẫn thành công
    public async Task LogoutAsync(string refreshToken, string? diaChiIp, string? userAgent)
    {
        var token = await _db.Set<RefreshToken>().FirstOrDefaultAsync(x => x.TokenHash == HashToken(refreshToken));
        if (token is null || token.ThuHoiLuc is not null) return;

        token.ThuHoiLuc = DateTime.UtcNow;
        token.LyDoThuHoi = LyDoThuHoiToken.DangXuat;
        var tenDangNhap = await _db.Set<TaiKhoan>().Where(x => x.Id == token.TaiKhoanId)
            .Select(x => x.TenDangNhap).FirstOrDefaultAsync();
        await GhiNhatKyAsync(token.TaiKhoanId, tenDangNhap ?? string.Empty, "Logout", true, diaChiIp, userAgent);
    }

    // Đăng xuất mọi thiết bị: thu hồi mọi refresh token và vô hiệu mọi access token đang dùng
    public async Task LogoutAllAsync(int taiKhoanId)
    {
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == taiKhoanId)
            ?? throw AppException.NotFound("tài khoản");
        var soPhien = await ThuHoiMoiPhienAsync(taiKhoanId, LyDoThuHoiToken.DangXuatTatCa, DateTime.UtcNow);
        // Đổi UpdatedAt làm claim auth_ver của mọi access token cũ không còn khớp
        account.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _nhatKy.Ghi($"thu hồi {soPhien} phiên");
    }

    private async Task<LoginResponse> TaoPhienAsync(TaiKhoan account, string? diaChiIp, string? userAgent)
    {
        var bay = DateTime.UtcNow;
        var accessHetHan = bay.AddMinutes(_jwt.ExpiryMinutes);
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        var refreshHetHan = bay.AddDays(_jwt.RefreshTokenDays);

        // Dọn các token đã hết hạn của tài khoản để bảng không phình mãi
        await _db.Set<RefreshToken>().Where(x => x.TaiKhoanId == account.Id && x.HetHan < bay).ExecuteDeleteAsync();
        _db.Add(new RefreshToken
        {
            TaiKhoanId = account.Id,
            TokenHash = HashToken(refreshToken),
            HetHan = refreshHetHan,
            DiaChiIp = Limit(diaChiIp, 45),
            UserAgent = Limit(userAgent, 512)
        });
        await _db.SaveChangesAsync();

        return new LoginResponse(
            TaoAccessToken(account, bay, accessHetHan),
            accessHetHan,
            account.Id,
            account.TenDangNhap,
            account.HoTen,
            account.VaiTro.ToString(),
            refreshToken,
            refreshHetHan);
    }

    private string TaoAccessToken(TaiKhoan account, DateTime issuedAt, DateTime expiresAt)
    {
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
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Thời điểm các lần đăng nhập sai còn tính (tăng dần): trong 30 phút gần nhất, sau lần đăng nhập thành công gần nhất
    /// và sau lần tài khoản được cập nhật (Admin đặt lại mật khẩu hoặc khóa/mở khóa thì gỡ chặn ngay).
    /// </summary>
    private async Task<List<DateTime>> LanDangNhapSaiGanDayAsync(string username, TaiKhoan? account, DateTime bay)
    {
        var tu = bay - ThoiGianTheoDoi;
        if (account?.UpdatedAt is { } capNhat && capNhat > tu) tu = capNhat;

        var dangNhap = _db.Set<NhatKyHoatDong>().AsNoTracking()
            .Where(x => x.TenDangNhap == username && x.HanhDong == HanhDongDangNhap && x.CreatedAt > tu);
        var thanhCongGanNhat = await dangNhap.Where(x => x.ThanhCong).MaxAsync(x => (DateTime?)x.CreatedAt);
        if (thanhCongGanNhat is { } lanThanhCong) tu = lanThanhCong;

        return await dangNhap.Where(x => !x.ThanhCong && x.CreatedAt > tu)
            .OrderBy(x => x.CreatedAt)
            .Select(x => x.CreatedAt)
            .ToListAsync();
    }

    private Task<int> ThuHoiMoiPhienAsync(int taiKhoanId, string lyDo, DateTime bay) =>
        _db.Set<RefreshToken>()
            .Where(x => x.TaiKhoanId == taiKhoanId && x.ThuHoiLuc == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ThuHoiLuc, bay)
                .SetProperty(x => x.LyDoThuHoi, lyDo)
                .SetProperty(x => x.UpdatedAt, bay));

    private async Task GhiNhatKyAsync(int? taiKhoanId, string tenDangNhap, string hanhDong, bool thanhCong,
        string? diaChiIp, string? userAgent, string? chiTiet = null)
    {
        _db.Add(new NhatKyHoatDong
        {
            TaiKhoanId = taiKhoanId,
            TenDangNhap = Limit(tenDangNhap, 50) ?? string.Empty,
            HanhDong = hanhDong,
            ThanhCong = thanhCong,
            PhuongThuc = "POST",
            DuongDan = hanhDong switch
            {
                "Refresh" or "RefreshTaiSuDung" => "/api/auth/refresh",
                "Logout" => "/api/auth/logout",
                _ => "/api/auth/login"
            },
            ChiTiet = Limit(chiTiet, 1000),
            DiaChiIp = Limit(diaChiIp, 45),
            UserAgent = Limit(userAgent, 512)
        });
        await _db.SaveChangesAsync();
    }

    // Token ngẫu nhiên 512 bit nên SHA-256 không cần salt; chỉ lưu hash để lộ CSDL cũng không dùng được token
    private static string HashToken(string? token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty)));

    public async Task ChangePasswordAsync(int taiKhoanId, ChangePasswordRequest request)
    {
        if (!HasValidBcryptPassword(request.MatKhauCu) || !HasValidBcryptPassword(request.MatKhauMoi))
            throw AppException.BadRequest("Mật khẩu không được vượt quá 72 byte UTF-8.");

        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == taiKhoanId);
        if (account is null || account.DaKhoa || !VerifyPassword(request.MatKhauCu, account.MatKhauHash))
            throw AppException.Unauthorized("Mật khẩu hiện tại không chính xác.");

        if (VerifyPassword(request.MatKhauMoi, account.MatKhauHash))
            throw AppException.BadRequest("Mật khẩu mới phải khác mật khẩu hiện tại.");

        account.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
        await _db.SaveChangesAsync();
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

    public async Task<AccountDto> GetAccountAsync(int id)
    {
        var account = await _db.Set<TaiKhoan>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return account is null ? throw AppException.NotFound("tài khoản") : ToAccountDto(account);
    }

    public Task<PagedResult<ActivityLogDto>> GetActivityLogsAsync(ActivityLogQuery query)
    {
        var logs = _db.Set<NhatKyHoatDong>().AsNoTracking();
        if (query.TaiKhoanId is int taiKhoanId) logs = logs.Where(x => x.TaiKhoanId == taiKhoanId);
        if (!string.IsNullOrWhiteSpace(query.HanhDong))
        {
            var hanhDong = query.HanhDong.Trim();
            logs = logs.Where(x => x.HanhDong.StartsWith(hanhDong));
        }
        if (query.ThanhCong is bool thanhCong) logs = logs.Where(x => x.ThanhCong == thanhCong);
        if (query.TuNgay is { } tuNgay) logs = logs.Where(x => x.CreatedAt >= tuNgay);
        if (query.DenNgay is { } denNgay) logs = logs.Where(x => x.CreatedAt <= denNgay);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            logs = logs.Where(x => x.TenDangNhap.Contains(search) || x.HanhDong.Contains(search) ||
                (x.DuongDan != null && x.DuongDan.Contains(search)) || (x.ChiTiet != null && x.ChiTiet.Contains(search)));
        }

        return logs.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Select(x => new ActivityLogDto(
                x.Id, x.TaiKhoanId, x.TenDangNhap, x.HanhDong, x.ThanhCong, x.PhuongThuc, x.DuongDan,
                x.MaPhanHoi, x.ChiTiet, x.DiaChiIp, x.UserAgent, x.CreatedAt))
            .ToPagedResultAsync(query);
    }

    public async Task<AccountDto> ToggleLockAsync(int id)
    {
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            throw AppException.NotFound("tài khoản");

        if (!account.DaKhoa && account.VaiTro == VaiTro.Admin && !await AnotherActiveAdminExistsAsync(id))
            throw AppException.Conflict("Không thể khóa Admin đang hoạt động cuối cùng.");

        account.DaKhoa = !account.DaKhoa;
        await _db.SaveChangesAsync();
        _nhatKy.Ghi($"{(account.DaKhoa ? "khóa" : "mở khóa")} tài khoản {account.TenDangNhap}");

        return ToAccountDto(account);
    }

    public async Task<AccountDto> LinkStudentAsync(int id, LinkStudentRequest request)
    {
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            throw AppException.NotFound("tài khoản");

        if (request.SinhVienId is { } sinhVienId)
            await EnsureCanLinkStudentAsync(id, account.VaiTro, sinhVienId);

        _nhatKy.Ghi($"tài khoản {account.TenDangNhap}: sinh viên {account.SinhVienId?.ToString() ?? "(không)"} -> {request.SinhVienId?.ToString() ?? "(không)"}");
        account.SinhVienId = request.SinhVienId;
        await _db.SaveChangesAsync();
        return ToAccountDto(account);
    }

    public async Task<AccountDto> LinkLecturerAsync(int id, LinkLecturerRequest request)
    {
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            throw AppException.NotFound("tài khoản");

        if (request.GiangVienId is { } giangVienId)
            await EnsureCanLinkLecturerAsync(id, account.VaiTro, giangVienId);

        _nhatKy.Ghi($"tài khoản {account.TenDangNhap}: giảng viên {account.GiangVienId?.ToString() ?? "(không)"} -> {request.GiangVienId?.ToString() ?? "(không)"}");
        account.GiangVienId = request.GiangVienId;
        await _db.SaveChangesAsync();
        return ToAccountDto(account);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
    {
        var username = request.TenDangNhap.Trim();
        var vaiTro = request.VaiTro ?? throw AppException.BadRequest("Vai trò là bắt buộc.");
        if (string.IsNullOrEmpty(username))
            throw AppException.BadRequest("Tên đăng nhập không được để trống.");
        if (!HasValidBcryptPassword(request.MatKhau))
            throw AppException.BadRequest("Mật khẩu không được vượt quá 72 byte UTF-8.");
        if (await _db.Set<TaiKhoan>().AnyAsync(x => x.TenDangNhap == username))
            throw AppException.Conflict($"Tên đăng nhập '{username}' đã tồn tại.");
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
            throw AppException.Conflict("Tên đăng nhập hoặc hồ sơ liên kết đã được dùng cho tài khoản khác.");
        }

        _logger.LogInformation("Account {AccountId} created with role {Role}", account.Id, account.VaiTro);
        _nhatKy.Ghi($"tạo tài khoản {account.TenDangNhap} (Id {account.Id}), vai trò {account.VaiTro}");
        return ToAccountDto(account);
    }

    // Đổi mật khẩu làm thay đổi UpdatedAt nên các token cũ của tài khoản bị thu hồi
    public async Task<AccountDto> ResetPasswordAsync(int id, ResetPasswordRequest request)
    {
        if (!HasValidBcryptPassword(request.MatKhauMoi))
            throw AppException.BadRequest("Mật khẩu không được vượt quá 72 byte UTF-8.");

        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            throw AppException.NotFound("tài khoản");

        account.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Password of account {AccountId} reset by admin", id);
        _nhatKy.Ghi($"đặt lại mật khẩu tài khoản {account.TenDangNhap}");
        return ToAccountDto(account);
    }

    public async Task<AccountDto> ChangeRoleAsync(int id, ChangeRoleRequest request)
    {
        var vaiTro = request.VaiTro ?? throw AppException.BadRequest("Vai trò là bắt buộc.");
        var account = await _db.Set<TaiKhoan>().FirstOrDefaultAsync(x => x.Id == id);
        if (account is null)
            throw AppException.NotFound("tài khoản");
        if (account.VaiTro == vaiTro)
            return ToAccountDto(account);

        if (account.VaiTro == VaiTro.Admin && !account.DaKhoa && !await AnotherActiveAdminExistsAsync(id))
            throw AppException.Conflict("Không thể đổi vai trò của Admin đang hoạt động cuối cùng.");

        // Liên kết hồ sơ chỉ có nghĩa với đúng vai trò, nên gỡ khi đổi sang vai trò khác
        _nhatKy.Ghi($"tài khoản {account.TenDangNhap}: vai trò {account.VaiTro} -> {vaiTro}");
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
            throw AppException.Conflict("Chỉ liên kết sinh viên cho tài khoản có vai trò SinhVien.");
        if (!await _db.Set<SinhVien>().AnyAsync(x => x.Id == sinhVienId))
            throw AppException.Conflict("Sinh viên không tồn tại.");
        if (await _db.Set<TaiKhoan>().AnyAsync(x => x.SinhVienId == sinhVienId && x.Id != accountId))
            throw AppException.Conflict("Sinh viên đã được liên kết với tài khoản khác.");
    }

    private async Task EnsureCanLinkLecturerAsync(int? accountId, VaiTro vaiTro, int giangVienId)
    {
        if (vaiTro != VaiTro.GiangVien)
            throw AppException.Conflict("Chỉ liên kết giảng viên cho tài khoản có vai trò GiangVien.");
        if (!await _db.Set<GiangVien>().AnyAsync(x => x.Id == giangVienId))
            throw AppException.Conflict("Giảng viên không tồn tại.");
        if (await _db.Set<TaiKhoan>().AnyAsync(x => x.GiangVienId == giangVienId && x.Id != accountId))
            throw AppException.Conflict("Giảng viên đã được liên kết với tài khoản khác.");
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
