using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

[Route("api/lop-hoc-phan")]
[Tags("Courses")]
public class LopHocPhanController(ILopHocPhanService service) : ApiControllerBase
{
    // CRS-02 Danh sách lớp học phần (lọc theo học kỳ, môn, giảng viên, trạng thái)
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<LopHocPhanDto>>>> GetPaged([FromQuery] LopHocPhanQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // CRS-02 Chi tiết lớp học phần
    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<LopHocPhanDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // CRS-02 Mở lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<LopHocPhanDto>>> Create([FromBody] LopHocPhanRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã mở lớp học phần.");

    // CRS-02 Cập nhật lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<LopHocPhanDto>>> Update(int id, [FromBody] LopHocPhanUpdateRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật lớp học phần.");

    // CRS-03 Phân công giảng viên cho lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}/giang-vien")]
    public Task<ActionResult<ApiResponse<LopHocPhanDto>>> PhanCongGiangVien(int id, [FromBody] PhanCongGiangVienRequest request) =>
        Execute(() => service.PhanCongGiangVienAsync(id, request), message: "Đã cập nhật phân công giảng viên.");

    // CRS-08 Bắt đầu học (MoDangKy -> DangHoc)
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/bat-dau-hoc")]
    public Task<ActionResult<ApiResponse<LopHocPhanDto>>> BatDauHoc(int id) =>
        Execute(() => service.BatDauHocAsync(id), message: "Lớp học phần đã chuyển sang đang học.");

    // CRS-08 Kết thúc lớp học phần (DangHoc -> DaKetThuc)
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/ket-thuc")]
    public Task<ActionResult<ApiResponse<LopHocPhanDto>>> KetThuc(int id) =>
        Execute(() => service.KetThucAsync(id), message: "Lớp học phần đã kết thúc.");

    // CRS-07 Hủy lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/huy")]
    public Task<ActionResult<ApiResponse<LopHocPhanDto>>> Huy(int id) =>
        Execute(() => service.HuyAsync(id), message: "Đã hủy lớp học phần.");

    // CRS-07 Gộp lớp học phần vào lớp đích
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/gop")]
    public Task<ActionResult<ApiResponse<GopLopResultDto>>> Gop(int id, [FromBody] GopLopRequest request) =>
        Execute(() => service.GopAsync(id, request), message: "Đã gộp lớp học phần.");

    // CRS-02 Xóa lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa lớp học phần.");
}
