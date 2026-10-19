using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

/// <summary>Sinh viên tự xem hồ sơ và lịch sử học tập của mình (tài khoản phải liên kết SinhVienId).</summary>
[Authorize(Roles = "SinhVien")]
[Route("api/sinh-vien/toi")]
[Tags("Students - Của tôi")]
public class SinhVienCuaToiController(ISinhVienService service, ICurrentUserService currentUser) : ApiControllerBase
{
    // STU-02
    [HttpGet]
    public Task<ActionResult<ApiResponse<SinhVienDetailDto>>> GetHoSo() =>
        Execute(async () => await service.GetByIdAsync(await currentUser.GetSinhVienIdAsync()));

    // STU-07
    [HttpGet("lich-su-hoc-tap")]
    public Task<ActionResult<ApiResponse<List<LichSuHocTapDto>>>> GetLichSuHocTap() =>
        Execute(async () => await service.GetLichSuHocTapAsync(await currentUser.GetSinhVienIdAsync()));
}
