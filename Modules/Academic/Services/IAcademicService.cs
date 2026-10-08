using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>Nghiệp vụ module Academic (Epic ACA).</summary>
public interface IAcademicService
{
    // ACA-02 CRUD bộ môn
    Task<object?> GetBoMonAsync(PagedQuery query);
    // ACA-03 CRUD ngành, chuyên ngành
    Task<object?> GetNganhAsync(PagedQuery query);
    // ACA-04 CRUD năm học, học kỳ
    Task<object?> GetHocKyAsync(PagedQuery query);
    // ACA-05 CRUD chương trình đào tạo
    Task<object?> GetChuongTrinhDaoTaoAsync(PagedQuery query);
}
