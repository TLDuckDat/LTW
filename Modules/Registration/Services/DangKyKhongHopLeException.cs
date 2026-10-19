using StudentManagement.Common;

namespace StudentManagement.Modules.Registration.Services;

/// <summary>Sinh viên không đủ điều kiện đăng ký (409); LyDo liệt kê từng điều kiện chưa đạt.</summary>
public class DangKyKhongHopLeException(IReadOnlyList<string> lyDo)
    : AppException(StatusCodes.Status409Conflict, "Không đủ điều kiện đăng ký: " + string.Join(" ", lyDo))
{
    public IReadOnlyList<string> LyDo { get; } = lyDo;
}
