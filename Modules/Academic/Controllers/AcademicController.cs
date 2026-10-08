using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/hoc-vu")]
[Tags("Academic")]
public class AcademicController(IAcademicService service) : ControllerBase
{
    private readonly IAcademicService _service = service;

    // ACA-02 CRUD bộ môn
    [HttpGet("/api/bo-mon")]
    public async Task<IActionResult> GetBoMon([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetBoMonAsync(query)));

    // ACA-03 CRUD ngành, chuyên ngành
    [HttpGet("/api/nganh")]
    public async Task<IActionResult> GetNganh([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetNganhAsync(query)));

    // ACA-04 CRUD năm học, học kỳ
    [HttpGet("/api/hoc-ky")]
    public async Task<IActionResult> GetHocKy([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetHocKyAsync(query)));

    // ACA-05 CRUD chương trình đào tạo
    [HttpGet("/api/chuong-trinh-dao-tao")]
    public async Task<IActionResult> GetChuongTrinhDaoTao([FromQuery] PagedQuery query)
        => Ok(ApiResponse<object>.Ok(await _service.GetChuongTrinhDaoTaoAsync(query)));
}
