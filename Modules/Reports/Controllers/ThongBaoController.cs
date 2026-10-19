using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Reports.Dtos;
using StudentManagement.Modules.Reports.Services;

namespace StudentManagement.Modules.Reports.Controllers;

/// <summary>RPT-05 Thông báo: mọi tài khoản đọc được thông báo đã đăng; Admin/PhongDaoTao quản lý.</summary>
[Authorize]
[Route("api/thong-bao")]
[Tags("Reports - Thông báo")]
public class ThongBaoController(IThongBaoService service) : ApiControllerBase
{
    // Nhân viên thấy cả thông báo hẹn giờ chưa đăng
    private bool ChiDaDang => !(User.IsInRole("Admin") || User.IsInRole("PhongDaoTao"));

    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<ThongBaoDto>>>> GetPaged([FromQuery] PagedQuery query) =>
        Execute(() => service.GetPagedAsync(query, ChiDaDang));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<ThongBaoDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id, ChiDaDang));

    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public Task<ActionResult<ApiResponse<ThongBaoDto>>> Create([FromBody] ThongBaoRequest request) =>
        Execute(() => service.CreateAsync(request, CurrentAccountId()), StatusCodes.Status201Created, "Đã tạo thông báo.");

    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public Task<ActionResult<ApiResponse<ThongBaoDto>>> Update(int id, [FromBody] ThongBaoRequest request) =>
        Execute(() => service.UpdateAsync(id, request), message: "Đã cập nhật thông báo.");

    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public Task<ActionResult<ApiResponse<object>>> Delete(int id) =>
        Execute(() => service.DeleteAsync(id), "Đã xóa thông báo.");

    // Claim "sub" đã được kiểm tra khi xác thực token (Program.cs, OnTokenValidated)
    private int CurrentAccountId() =>
        int.Parse(User.FindFirst("sub")!.Value, NumberStyles.None, CultureInfo.InvariantCulture);
}
