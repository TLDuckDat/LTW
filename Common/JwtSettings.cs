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
        public int ExpiryMinutes { get; set; } = 7 * 24 * 60;
    }
}
