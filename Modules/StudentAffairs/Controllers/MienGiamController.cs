using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

/// <summary>SA-04 Miễn giảm học phí. Sau khi thay đổi, tính lại học phí của học kỳ để áp dụng.</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/cong-tac-sinh-vien/mien-giam")]
[Tags("StudentAffairs - Miễn giảm")]
public class MienGiamController(IMienGiamService service) : ApiControllerBase
{
    private const string NhacTinhLai = " Hãy tính lại học phí của học kỳ để áp dụng.";

    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<MienGiamDto>>>> GetPaged([FromQuery] SinhVienHocKyQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<MienGiamDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    [HttpPost]
    public Task<ActionResult<ApiResponse<MienGiamDto>>> Create([FromBody] MienGiamRequest request) =>
        Execute(() => service.CreateAsync(request), StatusCodes.Status201Created, "Đã tạo hồ sơ miễn giảm." + NhacTinhLai);

    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<MienGiamDto>>> Update(int id, [FromBody] CapNhatMienGiamRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật hồ sơ miễn giảm." + NhacTinhLai);

    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa hồ sơ miễn giảm." + NhacTinhLai);
}
