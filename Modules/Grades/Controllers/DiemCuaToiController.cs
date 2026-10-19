using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

/// <summary>Sinh viên tự xem bảng điểm, cảnh báo học vụ và gửi phúc khảo cho điểm của mình.</summary>
[Authorize(Roles = "SinhVien")]
[Route("api/diem/toi")]
[Tags("Grades - Của tôi")]
public class DiemCuaToiController(
    IBangDiemService bangDiem,
    ICanhBaoHocVuService canhBao,
    IPhucKhaoService phucKhao,
    ICurrentUserService currentUser) : ApiControllerBase
{
    // GRADE-06
    [HttpGet("bang-diem")]
    public Task<ActionResult<ApiResponse<BangDiemSinhVienDto>>> GetBangDiem() =>
        Execute(async () => await bangDiem.GetBangDiemAsync(await currentUser.GetSinhVienIdAsync()));

    // GRADE-07
    [HttpGet("canh-bao")]
    public Task<ActionResult<ApiResponse<CanhBaoHocVuDto>>> GetCanhBao([FromQuery, Range(1, int.MaxValue)] int hocKyId) =>
        Execute(async () => await canhBao.GetCanhBaoAsync(await currentUser.GetSinhVienIdAsync(), hocKyId));

    // GRADE-05
    [HttpGet("phuc-khao")]
    public Task<ActionResult<ApiResponse<PagedResult<PhucKhaoDto>>>> GetPhucKhao(
        [FromQuery] PagedQuery query, [FromQuery] TrangThaiPhucKhao? trangThai) =>
        Execute(async () => await phucKhao.GetPagedAsync(query, trangThai, await currentUser.GetSinhVienIdAsync()));

    [HttpGet("phuc-khao/{id:int}")]
    public Task<ActionResult<ApiResponse<PhucKhaoDto>>> GetPhucKhaoById(int id) =>
        Execute(async () => await phucKhao.GetByIdAsync(id, await currentUser.GetSinhVienIdAsync()));

    [HttpPost("phuc-khao")]
    public Task<ActionResult<ApiResponse<PhucKhaoDto>>> CreatePhucKhao([FromBody] PhucKhaoRequest request) =>
        Execute(async () => await phucKhao.CreateAsync(request, await currentUser.GetSinhVienIdAsync()),
            StatusCodes.Status201Created, "Đã gửi yêu cầu phúc khảo.");
}
