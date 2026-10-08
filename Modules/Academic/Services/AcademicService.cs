using StudentManagement.Common;
using StudentManagement.Data;

namespace StudentManagement.Modules.Academic.Services;

public class AcademicService(AppDbContext db) : IAcademicService
{
    private readonly AppDbContext _db = db;

    // ACA-02 CRUD bộ môn
    public Task<object?> GetBoMonAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // ACA-03 CRUD ngành, chuyên ngành
    public Task<object?> GetNganhAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // ACA-04 CRUD năm học, học kỳ
    public Task<object?> GetHocKyAsync(PagedQuery query) => throw new NotImplementedException("TODO");

    // ACA-05 CRUD chương trình đào tạo
    public Task<object?> GetChuongTrinhDaoTaoAsync(PagedQuery query) => throw new NotImplementedException("TODO");
}
