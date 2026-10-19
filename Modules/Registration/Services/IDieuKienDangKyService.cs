using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Registration.Services;

/// <summary>Kết quả kiểm tra điều kiện đăng ký một lớp học phần của một sinh viên.</summary>
public class KetQuaKiemTraDangKy
{
    public List<string> LyDo { get; } = [];
    public LoaiDangKy LoaiDangKy { get; set; } = LoaiDangKy.HocMoi;
    public bool DuDieuKien => LyDo.Count == 0;
}

/// <summary>Kiểm tra sĩ số, môn tiên quyết, trùng lịch và các điều kiện liên quan trước khi đăng ký (REG-03).</summary>
public interface IDieuKienDangKyService
{
    /// <summary>
    /// Kiểm tra từng lớp học phần với sinh viên; trả về null nếu sinh viên không tồn tại.
    /// Dữ liệu của sinh viên được nạp một lần cho cả danh sách lớp.
    /// </summary>
    Task<Dictionary<int, KetQuaKiemTraDangKy>?> KiemTraAsync(int sinhVienId, IReadOnlyList<LopHocPhan> cacLop);
}
