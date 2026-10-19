using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

/// <summary>SA-02 Tính học phí, SA-03 Ghi nhận nộp học phí (nộp được nhiều lần).</summary>
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/cong-tac-sinh-vien/hoc-phi")]
[Tags("StudentAffairs - Học phí")]
public class HocPhiController(IHocPhiService service, ICurrentUserService currentUser) : ApiControllerBase
{
    [HttpGet]
    public Task<ActionResult<ApiResponse<PagedResult<HocPhiDto>>>> GetPaged([FromQuery] HocPhiQuery query) =>
        Execute(() => service.GetPagedAsync(query));

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiResponse<HocPhiDto>>> GetById(int id) =>
        Execute(() => service.GetByIdAsync(id));

    // Tính lại được nhiều lần: chỉ đổi số phải nộp, giữ nguyên số đã nộp
    [HttpPost("tinh/{hocKyId:int}")]
    public Task<ActionResult<ApiResponse<TinhHocPhiResultDto>>> TinhHocPhi(int hocKyId) =>
        Execute(() => service.TinhHocPhiAsync(hocKyId), message: "Đã tính học phí.");

    // Bỏ trống body hoặc SoTien để nộp toàn bộ số còn nợ
    [HttpPut("{id:int}/thanh-toan")]
    public Task<ActionResult<ApiResponse<ThanhToanResultDto>>> ThanhToan(
        int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ThanhToanRequest? request) =>
        Execute(() => service.ThanhToanAsync(id, request, currentUser.AccountId), message: "Đã ghi nhận nộp học phí.");

    [HttpGet("{id:int}/thanh-toan")]
    public Task<ActionResult<ApiResponse<IReadOnlyList<ThanhToanHocPhiDto>>>> GetLichSuThanhToan(int id) =>
        Execute(() => service.GetLichSuThanhToanAsync(id));
}
