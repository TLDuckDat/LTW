using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Auth.Entities;

namespace StudentManagement.Modules.Auth.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, AppDbContext db) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly AppDbContext _db = db;

    // Claim "sub" đã được kiểm tra khi xác thực token (Program.cs, OnTokenValidated)
    public int AccountId =>
        int.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value,
            NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            ? id
            : throw new AppException(StatusCodes.Status401Unauthorized, "Token không hợp lệ.");

    public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;

    public async Task<int> GetSinhVienIdAsync()
    {
        var accountId = AccountId;
        return await _db.Set<TaiKhoan>().AsNoTracking()
                   .Where(x => x.Id == accountId)
                   .Select(x => x.SinhVienId)
                   .FirstOrDefaultAsync()
               ?? throw AppException.Forbidden("Tài khoản chưa được liên kết với hồ sơ sinh viên.");
    }

    public async Task<int> GetGiangVienIdAsync()
    {
        var accountId = AccountId;
        return await _db.Set<TaiKhoan>().AsNoTracking()
                   .Where(x => x.Id == accountId)
                   .Select(x => x.GiangVienId)
                   .FirstOrDefaultAsync()
               ?? throw AppException.Forbidden("Tài khoản chưa được liên kết với hồ sơ giảng viên.");
    }

    public async Task<int?> GetGiangVienIdNeuLaGiangVienAsync() =>
        IsInRole(nameof(VaiTro.GiangVien)) ? await GetGiangVienIdAsync() : null;
}
