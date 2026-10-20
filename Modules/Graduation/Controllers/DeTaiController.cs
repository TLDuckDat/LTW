using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/tot-nghiep/de-tai")]
[Tags("Graduation - Đề tài")]
public class DeTaiController(IDeTaiService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<DeTaiDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<DeTaiDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost]
    public Task<ActionResult<ApiResponse<DeTaiDto>>> Create([FromBody] DeTaiRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã tạo đề tài.");

    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<DeTaiDto>>> Update(int id, [FromBody] DeTaiRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật đề tài.");

    [HttpPatch("{id:int}/dang-ky")]
    public Task<ActionResult<ApiResponse<DeTaiDto>>> DangKy(int id, [FromBody] DangKyDeTaiRequest request) =>
        Execute(() => service.DangKyAsync(id, request), message: "Đã đăng ký đề tài.");

    [HttpPatch("{id:int}/giang-vien")]
    public Task<ActionResult<ApiResponse<DeTaiDto>>> PhanCongGiangVien(
        int id, [FromBody] PhanCongHuongDanRequest request) =>
        Execute(() => service.PhanCongGiangVienAsync(id, request), message: "Đã phân công giảng viên hướng dẫn.");
}
