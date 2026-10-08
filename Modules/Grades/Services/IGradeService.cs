using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>Nghiệp vụ module Grades (Epic GRD).</summary>
public interface IGradeService
{
    // GRD-01 Nhập điểm theo lớp học phần
    Task<object?> NhapDiemAsync(int lopHocPhanId, NhapDiemRequest request);
    // GRD-02 Tính điểm tổng kết, điểm chữ
    Task<object?> TinhDiemAsync(int lopHocPhanId);
    // GRD-03 Khóa điểm
    Task<object?> KhoaDiemAsync(int lopHocPhanId);
    // GRD-04 Bảng điểm sinh viên, GPA
    Task<object?> GetBangDiemAsync(int sinhVienId);
    // GRD-05 Gửi / xử lý phúc khảo
    Task<object?> CreatePhucKhaoAsync(PhucKhaoRequest request);
}
