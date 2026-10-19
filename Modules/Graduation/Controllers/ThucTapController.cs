using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Graduation.Dtos;
using StudentManagement.Modules.Graduation.Services;

namespace StudentManagement.Modules.Graduation.Controllers;

[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/tot-nghiep/thuc-tap")]
[Tags("Graduation - Thực tập")]
public class ThucTapController(IThucTapService service) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<ThucTapDto>>>> GetPaged([FromQuery] ThucTapQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<ThucTapDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost]
    public Task<ActionResult<ApiResponse<ThucTapDto>>> Create([FromBody] ThucTapRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã đăng ký thực tập.");

    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<ThucTapDto>>> Update(int id, [FromBody] CapNhatThucTapRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật đăng ký.");

    [HttpPatch("{id:int}/huy")]
    public Task<ActionResult<ApiResponse<ThucTapDto>>> Huy(int id) =>
        Execute(() => service.HuyAsync(id), message: "Đã hủy đăng ký.");

    [HttpGet("{id:int}/ket-qua")]
    public Task<ActionResult<ApiResponse<KetQuaThucTapDto?>>> GetKetQua(int id) =>
        Execute(() => service.GetKetQuaAsync(id));

    [HttpPut("{id:int}/ket-qua")]
    public Task<ActionResult<ApiResponse<KetQuaThucTapDto>>> SetKetQua(int id, [FromBody] KetQuaThucTapRequest request) =>
        Execute(() => service.SetKetQuaAsync(id, request), message: "Đã ghi nhận kết quả thực tập.");
}
