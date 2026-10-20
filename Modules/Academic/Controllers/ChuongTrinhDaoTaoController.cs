using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[Route("api/chuong-trinh-dao-tao")]
[Tags("Academic")]
public class ChuongTrinhDaoTaoController(IChuongTrinhDaoTaoService service) : ApiControllerBase
{
    // ==========================================
    // EDU-04: QUẢN LÝ CHƯƠNG TRÌNH ĐÀO TẠO
    // ==========================================

    // Danh sách CTĐT (lọc theo ngành, khóa tuyển, tìm kiếm)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<ChuongTrinhDaoTaoDto>>>> GetPaged([FromQuery] ChuongTrinhDaoTaoFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // Chi tiết CTĐT (kèm danh sách môn học)
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<ChuongTrinhDaoTaoDetailDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // Thêm CTĐT
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<ChuongTrinhDaoTaoDto>>> Create([FromBody] ChuongTrinhDaoTaoRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo chương trình đào tạo.");

    // Cập nhật CTĐT
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<ChuongTrinhDaoTaoDto>>> Update(int id, [FromBody] ChuongTrinhDaoTaoRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật chương trình đào tạo.");

    // Xóa CTĐT
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa chương trình đào tạo.");

    // ==========================================
    // EDU-05: QUẢN LÝ MÔN HỌC TRONG CTĐT
    // ==========================================

    // Danh sách môn học trong CTĐT
    [HttpGet("{id:int}/mon-hoc")]
    public Task<ActionResult<ApiResponse<List<MonHocTrongCtdtDto>>>> GetDanhSachMonHoc(int id, [FromQuery] int? hocKyGoiY) =>
        Execute(() => service.GetDanhSachMonHocAsync(id, hocKyGoiY));

    // Thêm môn học vào CTĐT
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/mon-hoc")]
    public Task<ActionResult<ApiResponse<MonHocTrongCtdtDto>>> ThemMonHoc(int id, [FromBody] ThemMonHocVaoCtdtRequest request) =>
        Execute(() => service.ThemMonHocAsync(id, request), message: "Đã thêm môn học vào chương trình đào tạo.");

    // Cập nhật thông tin môn học trong CTĐT (đổi kỳ gợi ý, đổi bắt buộc/tự chọn)
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}/mon-hoc/{monHocId:int}")]
    public Task<ActionResult<ApiResponse<MonHocTrongCtdtDto>>> CapNhatMonHoc(int id, int monHocId, [FromBody] CapNhatMonHocTrongCtdtRequest request) =>
        Execute(() => service.CapNhatMonHocAsync(id, monHocId, request), message: "Đã cập nhật môn học trong chương trình đào tạo.");

    // Gỡ môn học khỏi CTĐT
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}/mon-hoc/{monHocId:int}")]
    public Task<ActionResult<ApiResponse<object>>> XoaMonHoc(int id, int monHocId) =>
        Execute(() => service.XoaMonHocAsync(id, monHocId), "Đã gỡ môn học khỏi chương trình đào tạo.");
}
