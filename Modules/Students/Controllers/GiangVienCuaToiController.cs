using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

/// <summary>Giảng viên tự xem hồ sơ của mình (tài khoản phải liên kết GiangVienId).</summary>
[Authorize(Roles = "GiangVien")]
[Route("api/giang-vien/toi")]
[Tags("Students - Của tôi")]
public class GiangVienCuaToiController(IGiangVienService service, ICurrentUserService currentUser) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<GiangVienDto>>> GetHoSo() =>
        Execute(async () => await service.GetByIdAsync(await currentUser.GetGiangVienIdAsync()));
}
