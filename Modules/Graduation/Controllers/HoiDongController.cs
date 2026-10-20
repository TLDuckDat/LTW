using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/tot-nghiep/hoi-dong")]
[Tags("Graduation - Hội đồng bảo vệ")]
public class HoiDongController(IHoiDongService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<HoiDongDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<HoiDongDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost]
    public Task<ActionResult<ApiResponse<HoiDongDto>>> Create([FromBody] HoiDongRequest request) =>
        ExecuteCreated(() => service.CreateAsync(request), x => x.Id, "Đã tạo hội đồng bảo vệ.");

    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<HoiDongDto>>> Update(int id, [FromBody] HoiDongRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật hội đồng bảo vệ.");

    [HttpGet("{id:int}/de-tai")]
    public Task<ActionResult<ApiResponse<PagedResult<DeTaiHoiDongDto>>>> GetDeTai(
        int id, [FromQuery] PagedQuery query) =>
        Execute(() => service.GetDeTaiAsync(id, query));

    [HttpPut("{id:int}/de-tai/{deTaiId:int}")]
    public Task<ActionResult<ApiResponse<PhanCongDeTaiDto>>> PhanCongDeTai(int id, int deTaiId) =>
        Execute(() => service.PhanCongDeTaiAsync(id, deTaiId), message: "Đã phân công đề tài vào hội đồng.");
}
