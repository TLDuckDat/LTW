namespace StudentManagement.Common
{
    /// <summary>
    /// Cấu hình JWT cho xác thực và ủy quyền.
    /// </summary>
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        /// <summary>Thời hạn access token (phút). Ngắn để token bị lộ nhanh hết tác dụng; dùng refresh token để gia hạn.</summary>
        public int ExpiryMinutes { get; set; } = 60;
        /// <summary>Thời hạn refresh token (ngày) kể từ lần cấp gần nhất.</summary>
        public int RefreshTokenDays { get; set; } = 7;
    }
}
