using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Students.Dtos;
using StudentManagement.Modules.Students.Services;

namespace StudentManagement.Modules.Students.Controllers;

[ApiController]
[Authorize(Roles = "Admin,PhongDaoTao,GiangVien")]
[Route("api/sinh-vien")]
[Tags("Students")]
public class SinhVienController(ISinhVienService service) : ControllerBase
{
    private readonly ISinhVienService _service = service;

    // STU-01 & STU-03: Danh sách sinh viên (phân trang, tìm kiếm & lọc)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SinhVienDto>>>> GetPaged([FromQuery] SinhVienFilterQuery query)
        => Ok(ApiResponse<PagedResult<SinhVienDto>>.Ok(await _service.GetPagedAsync(query)));

    // STU-02: Xem chi tiết hồ sơ sinh viên
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SinhVienDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<SinhVienDetailDto>.Fail("Không tìm thấy sinh viên."))
            : Ok(ApiResponse<SinhVienDetailDto>.Ok(result));
    }

    // STU-01: Thêm mới sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SinhVienDto>>> Create([FromBody] CreateSinhVienRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SinhVienDto>.Ok(result, "Đã thêm sinh viên thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<SinhVienDto>.Fail(ex.Message));
        }
    }

    // STU-01: Cập nhật thông tin sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<SinhVienDto>>> Update(int id, [FromBody] UpdateSinhVienRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<SinhVienDto>.Fail("Không tìm thấy sinh viên."))
                : Ok(ApiResponse<SinhVienDto>.Ok(result, "Đã cập nhật sinh viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<SinhVienDto>.Fail(ex.Message));
        }
    }

    // STU-01: Xóa sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            var success = await _service.DeleteAsync(id);
            return success
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa sinh viên."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy sinh viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // STU-04: Chuyển lớp hành chính cho sinh viên
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}/chuyen-lop")]
    public async Task<ActionResult<ApiResponse<object>>> ChuyenLop(int id, [FromBody] int? lopHanhChinhId)
    {
        try
        {
            var success = await _service.ChuyenLopAsync(id, lopHanhChinhId);
            return success
                ? Ok(ApiResponse<object>.Ok(null, "Đã chuyển lớp hành chính."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy sinh viên."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // STU-06: Đổi trạng thái học tập
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}/trang-thai")]
    public async Task<ActionResult<ApiResponse<SinhVienDto>>> ChangeStatus(int id, [FromBody] ChangeStatusRequest request)
    {
        var result = await _service.ChangeStatusAsync(id, request);
        return result is null
            ? NotFound(ApiResponse<SinhVienDto>.Fail("Không tìm thấy sinh viên."))
            : Ok(ApiResponse<SinhVienDto>.Ok(result, "Đã cập nhật trạng thái học tập."));
    }

    // STU-07: Tiếp nhận bảo lưu
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/bao-luu")]
    public async Task<ActionResult<ApiResponse<SinhVienDto>>> BaoLuu(int id, [FromBody] BaoLuuRequest request)
    {
        try
        {
            var result = await _service.BaoLuuAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<SinhVienDto>.Fail("Không tìm thấy sinh viên."))
                : Ok(ApiResponse<SinhVienDto>.Ok(result, "Đã bảo lưu sinh viên."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SinhVienDto>.Fail(ex.Message));
        }
    }

    // STU-07: Buộc thôi học / xin thôi học
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/thoi-hoc")]
    public async Task<ActionResult<ApiResponse<SinhVienDto>>> ThoiHoc(int id, [FromBody] ThoiHocRequest request)
    {
        try
        {
            var result = await _service.ThoiHocAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<SinhVienDto>.Fail("Không tìm thấy sinh viên."))
                : Ok(ApiResponse<SinhVienDto>.Ok(result, "Đã cập nhật thôi học sinh viên."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SinhVienDto>.Fail(ex.Message));
        }
    }

    // STU-07: Phục hồi / quay lại học tập sau bảo lưu
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("{id:int}/quay-lai-hoc")]
    public async Task<ActionResult<ApiResponse<SinhVienDto>>> QuayLaiHoc(int id, [FromQuery] string? ghiChu)
    {
        try
        {
            var result = await _service.QuayLaiHocAsync(id, ghiChu);
            return result is null
                ? NotFound(ApiResponse<SinhVienDto>.Fail("Không tìm thấy sinh viên."))
                : Ok(ApiResponse<SinhVienDto>.Ok(result, "Đã chuyển sinh viên về trạng thái đang học."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SinhVienDto>.Fail(ex.Message));
        }
    }

    // STU-07: Xem lịch sử học tập
    [HttpGet("{id:int}/lich-su-hoc-tap")]
    public async Task<ActionResult<ApiResponse<List<LichSuHocTapDto>>>> GetLichSuHocTap(int id)
    {
        var result = await _service.GetLichSuHocTapAsync(id);
        return Ok(ApiResponse<List<LichSuHocTapDto>>.Ok(result));
    }
}
