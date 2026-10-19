using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.Courses.Dtos;
using StudentManagement.Modules.Courses.Services;

namespace StudentManagement.Modules.Courses.Controllers;

/// <summary>Giảng viên xem các lớp học phần mình được phân công (lọc thêm theo học kỳ, môn, trạng thái).</summary>
[Authorize(Roles = "GiangVien")]
[Route("api/giang-vien/toi/lop-hoc-phan")]
[Tags("Courses - Của tôi")]
public class LopHocPhanCuaToiController(ILopHocPhanService service, ICurrentUserService currentUser) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<LopHocPhanDto>>>> GetPaged([FromQuery] LopHocPhanQuery query) =>
        Execute(async () =>
        {
            query.GiangVienId = await currentUser.GetGiangVienIdAsync();
            return await service.GetPagedAsync(query);
        });
}
