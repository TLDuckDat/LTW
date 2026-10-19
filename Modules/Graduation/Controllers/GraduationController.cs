using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/tot-nghiep")]
[Tags("Graduation - Xét tốt nghiệp")]
public class GraduationController(IGraduationService service) : ApiControllerBase
{
    [HttpPost("xet/{sinhVienId:int}")]
    public Task<ActionResult<ApiResponse<XetTotNghiepDto>>> XetTotNghiep(int sinhVienId) =>
        Execute(() => service.XetTotNghiepAsync(sinhVienId));

    [HttpGet("xet")]
    public Task<ActionResult<ApiResponse<PagedResult<XetTotNghiepDto>>>> GetPaged(
        [FromQuery] XetTotNghiepQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("xet/{sinhVienId:int}")]
    public Task<ActionResult<ApiResponse<ChiTietXetTotNghiepDto>>> GetBySinhVienId(int sinhVienId) =>
        Execute(() => service.GetBySinhVienIdAsync(sinhVienId));
}
