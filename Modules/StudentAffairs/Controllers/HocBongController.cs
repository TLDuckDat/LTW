using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

/// <summary>SA-05 Học bổng.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/cong-tac-sinh-vien/hoc-bong")]
[Tags("StudentAffairs - Học bổng")]
public class HocBongController(IHocBongService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<HocBongDto>>>> GetPaged([FromQuery] SinhVienHocKyQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    // Xét lại được nhiều lần; sinh viên đã được cấp không bị cấp trùng
    [HttpPost("xet/{hocKyId:int}")]
    public Task<ActionResult<ApiResponse<XetHocBongResultDto>>> XetHocBong(int hocKyId) =>
        Execute(() => service.XetHocBongAsync(hocKyId), message: "Đã xét học bổng.");

    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã thu hồi học bổng.");
}
