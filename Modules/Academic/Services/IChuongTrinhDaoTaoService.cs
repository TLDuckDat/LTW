using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;

namespace StudentManagement.Modules.Academic.Services;

public interface IChuongTrinhDaoTaoService
{
    // EDU-04: Quản lý chương trình đào tạo
    Task<PagedResult<ChuongTrinhDaoTaoDto>> GetPagedAsync(ChuongTrinhDaoTaoFilterQuery query);
    Task<ChuongTrinhDaoTaoDetailDto> GetByIdAsync(int id);
    Task<ChuongTrinhDaoTaoDto> CreateAsync(ChuongTrinhDaoTaoRequest request);
    Task<ChuongTrinhDaoTaoDto> UpdateAsync(int id, ChuongTrinhDaoTaoRequest request);
    Task DeleteAsync(int id);

    // EDU-05: Quản lý danh sách môn học trong CTĐT
    Task<List<MonHocTrongCtdtDto>> GetDanhSachMonHocAsync(int chuongTrinhDaoTaoId, int? hocKyGoiY = null);
    Task<MonHocTrongCtdtDto> ThemMonHocAsync(int chuongTrinhDaoTaoId, ThemMonHocVaoCtdtRequest request);
    Task<MonHocTrongCtdtDto> CapNhatMonHocAsync(int chuongTrinhDaoTaoId, int monHocId, CapNhatMonHocTrongCtdtRequest request);
    Task XoaMonHocAsync(int chuongTrinhDaoTaoId, int monHocId);
}
