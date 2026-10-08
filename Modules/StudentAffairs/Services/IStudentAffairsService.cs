using StudentManagement.Modules.StudentAffairs.Dtos;

namespace StudentManagement.Modules.StudentAffairs.Services;

/// <summary>Nghiệp vụ module StudentAffairs (Epic SA).</summary>
public interface IStudentAffairsService
{
    // SA-01 Nhập điểm rèn luyện
    Task<object?> NhapDiemRenLuyenAsync(DiemRenLuyenRequest request);
    // SA-02 Tính học phí theo học kỳ
    Task<object?> TinhHocPhiAsync(int hocKyId);
    // SA-03 Xác nhận thanh toán học phí
    Task<object?> ThanhToanAsync(int id);
    // SA-04 Quản lý miễn giảm
    Task<object?> CreateMienGiamAsync(MienGiamRequest request);
    // SA-05 Xét học bổng
    Task<object?> XetHocBongAsync(int hocKyId);
    // SA-06 Khen thưởng / kỷ luật
    Task<object?> CreateKhenThuongKyLuatAsync(KhenThuongKyLuatRequest request);
}
