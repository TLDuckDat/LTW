using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Auth.Services;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

/// <summary>Sinh viên tự xem học phí, điểm rèn luyện, học bổng và quyết định khen thưởng/kỷ luật của mình.</summary>
[Authorize(Roles = "SinhVien")]
[Route("api/cong-tac-sinh-vien/toi")]
[Tags("StudentAffairs - Của tôi")]
public class CongTacSinhVienCuaToiController(
    IHocPhiService hocPhi,
    IDiemRenLuyenService diemRenLuyen,
    IHocBongService hocBong,
    IKhenThuongKyLuatService khenThuongKyLuat,
    ICurrentUserService currentUser) : ApiControllerBase
{
    // SA-02
    [HttpGet("hoc-phi")]
    public Task<ActionResult<ApiResponse<PagedResult<HocPhiDto>>>> GetHocPhi([FromQuery] HocPhiQuery query) =>
        Execute(async () =>
        {
            query.SinhVienId = await currentUser.GetSinhVienIdAsync();
            return await hocPhi.GetPagedAsync(query);
        });

    // SA-03
    [HttpGet("hoc-phi/{id:int}/thanh-toan")]
    public Task<ActionResult<ApiResponse<IReadOnlyList<ThanhToanHocPhiDto>>>> GetLichSuThanhToan(int id) =>
        Execute(async () => await hocPhi.GetLichSuThanhToanAsync(id, await currentUser.GetSinhVienIdAsync()));

    // SA-01
    [HttpGet("diem-ren-luyen")]
    public Task<ActionResult<ApiResponse<PagedResult<DiemRenLuyenDto>>>> GetDiemRenLuyen([FromQuery] SinhVienHocKyQuery query) =>
        Execute(async () =>
        {
            query.SinhVienId = await currentUser.GetSinhVienIdAsync();
            return await diemRenLuyen.GetPagedAsync(query);
        });

    // SA-05
    [HttpGet("hoc-bong")]
    public Task<ActionResult<ApiResponse<PagedResult<HocBongDto>>>> GetHocBong([FromQuery] SinhVienHocKyQuery query) =>
        Execute(async () =>
        {
            query.SinhVienId = await currentUser.GetSinhVienIdAsync();
            return await hocBong.GetPagedAsync(query);
        });

    // SA-06
    [HttpGet("khen-thuong-ky-luat")]
    public Task<ActionResult<ApiResponse<PagedResult<KhenThuongKyLuatDto>>>> GetKhenThuongKyLuat(
        [FromQuery] KhenThuongKyLuatQuery query) =>
        Execute(async () =>
        {
            query.SinhVienId = await currentUser.GetSinhVienIdAsync();
            return await khenThuongKyLuat.GetPagedAsync(query);
        });
}
