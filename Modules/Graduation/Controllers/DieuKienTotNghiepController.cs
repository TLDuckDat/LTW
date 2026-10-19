using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/tot-nghiep/dieu-kien")]
[Tags("Graduation - Điều kiện tốt nghiệp")]
public class DieuKienTotNghiepController(IDieuKienTotNghiepService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<DieuKienTotNghiepDto>>>> GetPaged(
        [FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{sinhVienId:int}")]
    public Task<ActionResult<ApiResponse<DieuKienTotNghiepDto>>> GetBySinhVienId(int sinhVienId) =>
        Execute(() => service.GetBySinhVienIdAsync(sinhVienId));
}
