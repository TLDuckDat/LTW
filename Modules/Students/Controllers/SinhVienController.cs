using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao,GiangVien")]
[Route("api/sinh-vien")]
[Tags("Students")]
public class SinhVienController(ISinhVienService service) : ApiControllerBase
{
    // STU-01 & STU-03: Danh sách sinh viên (phân trang, tìm kiếm & lọc)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<SinhVienDto>>>> GetPaged([FromQuery] SinhVienFilterQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // STU-02: Xem chi tiết hồ sơ sinh viên
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<SinhVienDetailDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // STU-01: Thêm mới sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<SinhVienDto>>> Create([FromBody] CreateSinhVienRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã thêm sinh viên thành công.");

    // STU-01: Cập nhật thông tin sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<SinhVienDto>>> Update(int id, [FromBody] UpdateSinhVienRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật sinh viên.");

    // STU-01: Xóa sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa sinh viên.");

    // STU-04: Chuyển lớp hành chính cho sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}/chuyen-lop")]
    public Task<ActionResult<ApiResponse<object>>> ChuyenLop(int id, [FromBody] int? lopHanhChinhId) =>
        Execute(() => service.ChuyenLopAsync(id, lopHanhChinhId), "Đã chuyển lớp hành chính.");

    // STU-06: Đổi trạng thái học tập
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}/trang-thai")]
    public Task<ActionResult<ApiResponse<SinhVienDto>>> ChangeStatus(int id, [FromBody] ChangeStatusRequest request) =>
        Execute(() => service.ChangeStatusAsync(id, request), message: "Đã cập nhật trạng thái học tập.");

    // STU-07: Tiếp nhận bảo lưu
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/bao-luu")]
    public Task<ActionResult<ApiResponse<SinhVienDto>>> BaoLuu(int id, [FromBody] BaoLuuRequest request) =>
        Execute(() => service.BaoLuuAsync(id, request), message: "Đã bảo lưu sinh viên.");

    // STU-07: Buộc thôi học / xin thôi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/thoi-hoc")]
    public Task<ActionResult<ApiResponse<SinhVienDto>>> ThoiHoc(int id, [FromBody] ThoiHocRequest request) =>
        Execute(() => service.ThoiHocAsync(id, request), message: "Đã cập nhật thôi học sinh viên.");

    // STU-07: Phục hồi / quay lại học tập sau bảo lưu
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/quay-lai-hoc")]
    public Task<ActionResult<ApiResponse<SinhVienDto>>> QuayLaiHoc(int id, [FromQuery] string? ghiChu) =>
        Execute(() => service.QuayLaiHocAsync(id, ghiChu), message: "Đã chuyển sinh viên về trạng thái đang học.");

    // STU-07: Xem lịch sử học tập
    [HttpGet("{id:int}/lich-su-hoc-tap")]
    public Task<ActionResult<ApiResponse<List<LichSuHocTapDto>>>> GetLichSuHocTap(int id) =>
        Execute(() => service.GetLichSuHocTapAsync(id));
}
