using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>Điểm theo lớp học phần: nhập (GRADE-02) → tính tổng kết (GRADE-03) → duyệt và khóa (GRADE-04).</summary>
public interface IDiemService
{
    Task<NhapDiemResultDto> NhapDiemAsync(int lopHocPhanId, NhapDiemRequest request);
    Task<TinhDiemResultDto> TinhDiemAsync(int lopHocPhanId);
    Task<KhoaDiemResultDto> KhoaDiemAsync(int lopHocPhanId);
}
