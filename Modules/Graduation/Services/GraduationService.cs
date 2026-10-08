using StudentManagement.Data;
using StudentManagement.Modules.Graduation.Dtos;

namespace StudentManagement.Modules.Graduation.Services;

public class GraduationService(AppDbContext db) : IGraduationService
{
    private readonly AppDbContext _db = db;

    // GRAD-01 Đăng ký / chấm thực tập
    public Task<object?> CreateThucTapAsync(ThucTapRequest request) => throw new NotImplementedException("TODO");

    // GRAD-02 Đăng ký đề tài khóa luận
    public Task<object?> CreateDeTaiAsync(DeTaiRequest request) => throw new NotImplementedException("TODO");

    // GRAD-03 Lập hội đồng bảo vệ
    public Task<object?> CreateHoiDongAsync(HoiDongRequest request) => throw new NotImplementedException("TODO");

    // GRAD-04 Xét tốt nghiệp
    public Task<object?> XetTotNghiepAsync(int sinhVienId) => throw new NotImplementedException("TODO");
}
