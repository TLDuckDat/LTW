using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

/// <summary>Nghiệp vụ module Graduation (Epic GRAD).</summary>
public interface IGraduationService
{
    // GRAD-01 Đăng ký / chấm thực tập
    Task<object?> CreateThucTapAsync(ThucTapRequest request);
    // GRAD-02 Đăng ký đề tài khóa luận
    Task<object?> CreateDeTaiAsync(DeTaiRequest request);
    // GRAD-03 Lập hội đồng bảo vệ
    Task<object?> CreateHoiDongAsync(HoiDongRequest request);
    // GRAD-04 Xét tốt nghiệp
    Task<object?> XetTotNghiepAsync(int sinhVienId);
}
