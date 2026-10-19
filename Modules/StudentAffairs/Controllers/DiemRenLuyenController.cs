using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

/// <summary>SA-01 Điểm rèn luyện.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/cong-tac-sinh-vien/diem-ren-luyen")]
[Tags("StudentAffairs - Điểm rèn luyện")]
public class DiemRenLuyenController(IDiemRenLuyenService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<DiemRenLuyenDto>>>> GetPaged([FromQuery] SinhVienHocKyQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<DiemRenLuyenDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost]
    public Task<ActionResult<ApiResponse<DiemRenLuyenDto>>> Create([FromBody] DiemRenLuyenRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã nhập điểm rèn luyện.");

    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<DiemRenLuyenDto>>> Update(int id, [FromBody] CapNhatDiemRenLuyenRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật điểm rèn luyện.");

    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa điểm rèn luyện.");
}
