using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/tot-nghiep/ket-qua-bao-ve")]
[Tags("Graduation - Kết quả bảo vệ")]
public class KetQuaBaoVeController(IKetQuaBaoVeService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<KetQuaBaoVeDto>>>> GetPaged(
        [FromQuery] KetQuaBaoVeQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{deTaiId:int}")]
    public Task<ActionResult<ApiResponse<KetQuaBaoVeDto>>> GetByDeTaiId(int deTaiId) =>
        Execute(() => service.GetByDeTaiIdAsync(deTaiId));

    [HttpPut("{deTaiId:int}")]
    public Task<ActionResult<ApiResponse<KetQuaBaoVeDto>>> Save(
        int deTaiId, [FromBody] NhapKetQuaBaoVeRequest request) =>
        Execute(() => service.SaveAsync(deTaiId, request), message: "Đã lưu kết quả bảo vệ.");
}
