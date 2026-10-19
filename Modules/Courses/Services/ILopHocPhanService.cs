using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;

namespace StudentManagement.Modules.Courses.Services;

/// <summary>Quản lý lớp học phần: mở lớp theo học kỳ, sĩ số, phân công giảng viên, hủy/gộp lớp.</summary>
public interface ILopHocPhanService
{
    // CRS-02
    Task<PagedResult<LopHocPhanDto>> GetPagedAsync(LopHocPhanQuery query);
    Task<LopHocPhanDto?> GetByIdAsync(int id);
    Task<LopHocPhanDto> CreateAsync(LopHocPhanRequest request);
    Task<LopHocPhanDto?> UpdateAsync(int id, LopHocPhanUpdateRequest request);
    Task<bool> DeleteAsync(int id);

    Task<LopHocPhanDto?> CapNhatSiSoToiDaAsync(int id, int siSoToiDa);

    // CRS-03
    Task<LopHocPhanDto?> PhanCongGiangVienAsync(int id, PhanCongGiangVienRequest request);

    // CRS-08 Vòng đời lớp: MoDangKy -> DangHoc -> DaKetThuc
    Task<LopHocPhanDto?> BatDauHocAsync(int id);
    Task<LopHocPhanDto?> KetThucAsync(int id);

    // CRS-07
    Task<LopHocPhanDto?> HuyAsync(int id);
    Task<GopLopResultDto?> GopAsync(int id, GopLopRequest request);
}
