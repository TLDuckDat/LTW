namespace StudentManagement.Modules.Registration.Services;

/// <summary>Sinh viên không đủ điều kiện đăng ký; LyDo liệt kê từng điều kiện chưa đạt.</summary>
public class DangKyKhongHopLeException(IReadOnlyList<string> lyDo)
    : InvalidOperationException("Không đủ điều kiện đăng ký: " + string.Join(" ", lyDo))
{
    public IReadOnlyList<string> LyDo { get; } = lyDo;
}
