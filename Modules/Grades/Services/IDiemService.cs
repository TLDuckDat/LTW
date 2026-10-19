using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>
/// Điểm theo lớp học phần: nhập (GRADE-02) → tính tổng kết (GRADE-03) → duyệt và khóa (GRADE-04).
/// chiGiangVienId: khi có giá trị, chỉ thao tác được trên lớp giảng viên đó được phân công (403 nếu không).
/// </summary>
public interface IDiemService
{
    Task<BangDiemLopDto> GetBangDiemLopAsync(int lopHocPhanId, int? chiGiangVienId = null);
    Task<NhapDiemResultDto> NhapDiemAsync(int lopHocPhanId, NhapDiemRequest request, int? chiGiangVienId = null);
    Task<TinhDiemResultDto> TinhDiemAsync(int lopHocPhanId, int? chiGiangVienId = null);
    Task<KhoaDiemResultDto> KhoaDiemAsync(int lopHocPhanId);
}
