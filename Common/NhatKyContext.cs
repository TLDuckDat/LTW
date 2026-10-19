namespace StudentManagement.Common;

/// <summary>
/// Chi tiết nghiệp vụ của request hiện tại (scoped) để ghi vào nhật ký hoạt động, ví dụ
/// "vai trò SinhVien -> GiangVien". Service gọi <see cref="Ghi"/>; middleware nhật ký đọc khi request kết thúc.
/// Không ghi mật khẩu hay dữ liệu nhạy cảm vào đây.
/// </summary>
public class NhatKyContext
{
    private readonly List<string> _chiTiet = [];

    public IReadOnlyList<string> ChiTiet => _chiTiet;

    public void Ghi(string chiTiet) => _chiTiet.Add(chiTiet);
}
