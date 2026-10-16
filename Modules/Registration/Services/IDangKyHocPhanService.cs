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

    // REG-03, REG-06: loại đăng ký (học mới/học lại/học cải thiện) tự xác định theo lịch sử học
    Task<DangKyDto?> DangKyAsync(int sinhVienId, int lopHocPhanId, bool xacNhanHocCaiThien = false);

    // REG-05
    Task<DaDangKyDto?> GetDaDangKyAsync(int sinhVienId, int? hocKyId);
    Task<ThoiKhoaBieuDto?> GetThoiKhoaBieuAsync(int sinhVienId, int? hocKyId);

    // REG-07
    Task<PagedResult<SiSoLopDto>> GetSiSoAsync(SiSoQuery query);
    Task<SiSoLopDto?> CapNhatSiSoToiDaAsync(int lopHocPhanId, int siSoToiDa);

    // REG-04: chiSinhVienId != null thì chỉ hủy được lượt đăng ký của sinh viên đó (sinh viên tự hủy)
    Task<DangKyDto?> HuyDangKyAsync(int dangKyId, int? chiSinhVienId);
}
