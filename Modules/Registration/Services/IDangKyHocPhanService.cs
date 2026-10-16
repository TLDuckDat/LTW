using StudentManagement.Common;
using StudentManagement.Modules.Registration.Dtos;

namespace StudentManagement.Modules.Registration.Services;

/// <summary>Quy trình đăng ký học phần của sinh viên (REG-02 đến REG-07).</summary>
public interface IDangKyHocPhanService
{
    /// <summary>Hồ sơ sinh viên gắn với tài khoản; null nếu chưa liên kết.</summary>
    Task<int?> GetSinhVienIdCuaTaiKhoanAsync(int taiKhoanId);

    // REG-02
    Task<PagedResult<LopMoDto>?> GetLopMoAsync(int sinhVienId, LopMoQuery query);

    // REG-03
    Task<DangKyDto?> DangKyAsync(int sinhVienId, int lopHocPhanId);

    // REG-04: chiSinhVienId != null thì chỉ hủy được lượt đăng ký của sinh viên đó (sinh viên tự hủy)
    Task<DangKyDto?> HuyDangKyAsync(int dangKyId, int? chiSinhVienId);
}
