using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

[ApiController]
[Authorize(Roles = "Admin,PhongDaoTao")]
[Route("api/cong-tac-sinh-vien")]
[Tags("StudentAffairs")]
public class StudentAffairsController(IStudentAffairsService service) : ControllerBase
{
    private readonly IStudentAffairsService _service = service;

    // SA-01 Nhập điểm rèn luyện
    [HttpGet("diem-ren-luyen")]
    public async Task<IActionResult> GetDiemRenLuyen(
       [FromQuery] int? sinhVienId,
       [FromQuery] int? hocKyId)
    {
        var result = await _service.GetDiemRenLuyenAsync(
            sinhVienId, hocKyId);

        return Ok(ApiResponse<object>.Ok(result));
    }

    // Nhập điểm rèn luyện
    [HttpPost("diem-ren-luyen")]
    public async Task<IActionResult> NhapDiemRenLuyen(
        [FromBody] DiemRenLuyenRequest request)
    {
        try
        {
            var result = await _service.NhapDiemRenLuyenAsync(request);

            return Ok(ApiResponse<object>.Ok(
                result, "Đã nhập điểm rèn luyện."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // SA-02 Tính học phí theo học kỳ
    [HttpPost("hoc-phi/tinh/{hocKyId:int}")]
    public async Task<IActionResult> TinhHocPhi(int hocKyId)
    {
        try
        {
            var result = await _service.TinhHocPhiAsync(hocKyId);

            return Ok(ApiResponse<object>.Ok(
                result, "Đã tính học phí thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
    // Xem danh sách học phí
    [HttpGet("hoc-phi")]
    public async Task<IActionResult> GetHocPhi(
        [FromQuery] int? sinhVienId,
        [FromQuery] int? hocKyId)
    {
        var result = await _service.GetHocPhiAsync(
            sinhVienId, hocKyId);

        return Ok(ApiResponse<object>.Ok(result));
    }


    // SA-03 Xác nhận thanh toán học phí
    [HttpPut("hoc-phi/{id:int}/thanh-toan")]
    public async Task<IActionResult> ThanhToan(int id)
    {
        try
        {
            var result = await _service.ThanhToanAsync(id);

            return Ok(ApiResponse<object>.Ok(
                result, "Đã xác nhận thanh toán học phí."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // SA-04 Quản lý miễn giảm
    [HttpPost("mien-giam")]
    public async Task<IActionResult> CreateMienGiam(
        [FromBody] MienGiamRequest request)
    {
        try
        {
            var result = await _service.CreateMienGiamAsync(request);

            return Ok(ApiResponse<object>.Ok(
                result, "Tạo miễn giảm học phí thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // SA-05: Xét học bổng
    [HttpPost("hoc-bong/xet/{hocKyId:int}")]
    public async Task<IActionResult> XetHocBong(int hocKyId)
    {
        try
        {
            var result = await _service.XetHocBongAsync(hocKyId);

            return Ok(ApiResponse<object>.Ok(
                result, "Xét học bổng thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // SA-06 Khen thưởng / kỷ luật
    [HttpPost("khen-thuong-ky-luat")]
    public async Task<IActionResult> CreateKhenThuongKyLuat(
    [FromBody] KhenThuongKyLuatRequest request)
    {
        try
        {
            var result = await _service.CreateKhenThuongKyLuatAsync(request);

            return Ok(ApiResponse<object>.Ok(
                result, "Tạo quyết định thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
