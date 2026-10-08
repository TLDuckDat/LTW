using StudentManagement.Data;
using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

public class GradeService(AppDbContext db) : IGradeService
{
    private readonly AppDbContext _db = db;

    // GRD-01 Nhập điểm theo lớp học phần
    public Task<object?> NhapDiemAsync(int lopHocPhanId, NhapDiemRequest request) => throw new NotImplementedException("TODO");

    // GRD-02 Tính điểm tổng kết, điểm chữ
    public Task<object?> TinhDiemAsync(int lopHocPhanId) => throw new NotImplementedException("TODO");

    // GRD-03 Khóa điểm
    public Task<object?> KhoaDiemAsync(int lopHocPhanId) => throw new NotImplementedException("TODO");

    // GRD-04 Bảng điểm sinh viên, GPA
    public Task<object?> GetBangDiemAsync(int sinhVienId) => throw new NotImplementedException("TODO");

    // GRD-05 Gửi / xử lý phúc khảo
    public Task<object?> CreatePhucKhaoAsync(PhucKhaoRequest request) => throw new NotImplementedException("TODO");
}
