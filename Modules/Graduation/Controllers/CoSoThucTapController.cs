using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/tot-nghiep/co-so-thuc-tap")]
[Tags("Graduation - Cơ sở thực tập")]
public class CoSoThucTapController(ICoSoThucTapService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<CoSoThucTapDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<CoSoThucTapDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost]
    public Task<ActionResult<ApiResponse<CoSoThucTapDto>>> Create([FromBody] CoSoThucTapRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo cơ sở thực tập.");

    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<CoSoThucTapDto>>> Update(int id, [FromBody] CoSoThucTapRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật cơ sở thực tập.");

    [HttpPut("{id:int}/trang-thai")]
    public Task<ActionResult<ApiResponse<CoSoThucTapDto>>> SetTrangThai(int id, [FromBody] TrangThaiCoSoRequest request) =>
        Execute(() => service.SetTrangThaiAsync(id, request), message: "Đã cập nhật trạng thái tiếp nhận.");

    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<bool>>> Delete(int id) =>
        Execute(async () => { await service.DeleteAsync(id); return true; }, message: "Đã xóa cơ sở thực tập.");
}
