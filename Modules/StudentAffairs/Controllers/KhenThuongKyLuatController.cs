using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

/// <summary>SA-06 Khen thưởng / kỷ luật.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/cong-tac-sinh-vien/khen-thuong-ky-luat")]
[Tags("StudentAffairs - Khen thưởng, kỷ luật")]
public class KhenThuongKyLuatController(IKhenThuongKyLuatService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<KhenThuongKyLuatDto>>>> GetPaged([FromQuery] KhenThuongKyLuatQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<KhenThuongKyLuatDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost]
    public Task<ActionResult<ApiResponse<KhenThuongKyLuatDto>>> Create([FromBody] KhenThuongKyLuatRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã lưu quyết định.");

    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<KhenThuongKyLuatDto>>> Update(int id, [FromBody] KhenThuongKyLuatRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật quyết định.");

    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa quyết định.");
}
