using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.StudentAffairs.Dtos;
using StudentManagement.Modules.StudentAffairs.Services;

namespace StudentManagement.Modules.StudentAffairs.Controllers;

[ApiController]
[Route("api/cong-tac-sinh-vien")]
[Tags("StudentAffairs")]
public class StudentAffairsController(IStudentAffairsService service) : ControllerBase
{
    private readonly IStudentAffairsService _service = service;

    // SA-01 Nhập điểm rèn luyện
    [HttpPost("diem-ren-luyen")]
    public async Task<IActionResult> NhapDiemRenLuyen([FromBody] DiemRenLuyenRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.NhapDiemRenLuyenAsync(request)));

    // SA-02 Tính học phí theo học kỳ
    [HttpPost("hoc-phi/tinh/{hocKyId:int}")]
    public async Task<IActionResult> TinhHocPhi(int hocKyId)
        => Ok(ApiResponse<object>.Ok(await _service.TinhHocPhiAsync(hocKyId)));

    // SA-03 Xác nhận thanh toán học phí
    [HttpPut("hoc-phi/{id:int}/thanh-toan")]
    public async Task<IActionResult> ThanhToan(int id)
        => Ok(ApiResponse<object>.Ok(await _service.ThanhToanAsync(id)));

    // SA-04 Quản lý miễn giảm
    [HttpPost("mien-giam")]
    public async Task<IActionResult> CreateMienGiam([FromBody] MienGiamRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateMienGiamAsync(request)));

    // SA-05 Xét học bổng
    [HttpPost("hoc-bong/xet/{hocKyId:int}")]
    public async Task<IActionResult> XetHocBong(int hocKyId)
        => Ok(ApiResponse<object>.Ok(await _service.XetHocBongAsync(hocKyId)));

    // SA-06 Khen thưởng / kỷ luật
    [HttpPost("khen-thuong-ky-luat")]
    public async Task<IActionResult> CreateKhenThuongKyLuat([FromBody] KhenThuongKyLuatRequest request)
        => Ok(ApiResponse<object>.Ok(await _service.CreateKhenThuongKyLuatAsync(request)));
}
