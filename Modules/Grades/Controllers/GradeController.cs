using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;
using StudentManagement.Modules.Grades.Services;

namespace StudentManagement.Modules.Grades.Controllers;

[ApiController]
[Route("api/diem")]
[Tags("Grades")]
public class GradeController(IGradeService service) : ControllerBase
{
    private readonly IGradeService _service = service;

    // GRADE-01 Danh sách sinh viên của lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpGet("lop-hoc-phan/{lopHocPhanId:int}/sinh-vien")]
    public async Task<ActionResult<ApiResponse<PagedResult<SinhVienTrongLopDto>>>> GetDanhSachSinhVien(int lopHocPhanId, [FromQuery] PagedQuery query)
    {
        var result = await _service.GetDanhSachSinhVienAsync(lopHocPhanId, query);
        return result is null
            ? NotFound(ApiResponse<PagedResult<SinhVienTrongLopDto>>.Fail("Không tìm thấy lớp học phần."))
            : Ok(ApiResponse<PagedResult<SinhVienTrongLopDto>>.Ok(result));
    }

    // GRADE-01 Thêm sinh viên vào lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("lop-hoc-phan/{lopHocPhanId:int}/sinh-vien")]
    public async Task<ActionResult<ApiResponse<DangKyHocPhanDto>>> ThemSinhVien(int lopHocPhanId, [FromBody] ThemSinhVienRequest request)
    {
        try
        {
            var result = await _service.ThemSinhVienAsync(lopHocPhanId, request);
            return result is null
                ? NotFound(ApiResponse<DangKyHocPhanDto>.Fail("Không tìm thấy lớp học phần hoặc sinh viên."))
                : StatusCode(StatusCodes.Status201Created, ApiResponse<DangKyHocPhanDto>.Ok(result, "Đã thêm sinh viên vào lớp."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<DangKyHocPhanDto>.Fail(ex.Message));
        }
    }

    // GRADE-01 Chuyển sinh viên sang lớp học phần khác cùng môn
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("lop-hoc-phan/{lopHocPhanId:int}/sinh-vien/{dangKyHocPhanId:int}")]
    public async Task<ActionResult<ApiResponse<DangKyHocPhanDto>>> ChuyenLop(int lopHocPhanId, int dangKyHocPhanId, [FromBody] ChuyenLopRequest request)
    {
        try
        {
            var result = await _service.ChuyenLopAsync(lopHocPhanId, dangKyHocPhanId, request);
            return result is null
                ? NotFound(ApiResponse<DangKyHocPhanDto>.Fail("Không tìm thấy lớp học phần, lượt đăng ký hoặc lớp mới."))
                : Ok(ApiResponse<DangKyHocPhanDto>.Ok(result, "Đã chuyển sinh viên sang lớp mới."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<DangKyHocPhanDto>.Fail(ex.Message));
        }
    }

    // GRADE-01 Hủy sinh viên khỏi lớp học phần (không xóa vật lý)
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("lop-hoc-phan/{lopHocPhanId:int}/sinh-vien/{dangKyHocPhanId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> HuyDangKy(int lopHocPhanId, int dangKyHocPhanId)
    {
        try
        {
            return await _service.HuyDangKyAsync(lopHocPhanId, dangKyHocPhanId)
                ? Ok(ApiResponse<object>.Ok(null, "Đã hủy đăng ký của sinh viên trong lớp."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp học phần hoặc lượt đăng ký."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // GRADE-02 Nhập và cập nhật điểm theo lớp học phần
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("lop-hoc-phan/{lopHocPhanId:int}")]
    public async Task<ActionResult<ApiResponse<NhapDiemResultDto>>> NhapDiem(int lopHocPhanId, [FromBody] NhapDiemRequest request)
    {
        try
        {
            var result = await _service.NhapDiemAsync(lopHocPhanId, request);
            return result is null
                ? NotFound(ApiResponse<NhapDiemResultDto>.Fail("Không tìm thấy lớp học phần."))
                : Ok(ApiResponse<NhapDiemResultDto>.Ok(result, "Đã lưu điểm."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<NhapDiemResultDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<NhapDiemResultDto>.Fail(ex.Message));
        }
    }

    // GRADE-03 Tính điểm tổng kết, điểm chữ
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("lop-hoc-phan/{lopHocPhanId:int}/tinh-diem")]
    public async Task<ActionResult<ApiResponse<TinhDiemResultDto>>> TinhDiem(int lopHocPhanId)
    {
        try
        {
            var result = await _service.TinhDiemAsync(lopHocPhanId);
            return result is null
                ? NotFound(ApiResponse<TinhDiemResultDto>.Fail("Không tìm thấy lớp học phần."))
                : Ok(ApiResponse<TinhDiemResultDto>.Ok(result, "Đã tính điểm tổng kết."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<TinhDiemResultDto>.Fail(ex.Message));
        }
    }

    // GRADE-04 Duyệt và khóa bảng điểm
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("lop-hoc-phan/{lopHocPhanId:int}/khoa")]
    public async Task<ActionResult<ApiResponse<KhoaDiemResultDto>>> KhoaDiem(int lopHocPhanId)
    {
        try
        {
            var result = await _service.KhoaDiemAsync(lopHocPhanId);
            return result is null
                ? NotFound(ApiResponse<KhoaDiemResultDto>.Fail("Không tìm thấy lớp học phần."))
                : Ok(ApiResponse<KhoaDiemResultDto>.Ok(result, "Đã duyệt và khóa bảng điểm."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<KhoaDiemResultDto>.Fail(ex.Message));
        }
    }

    // GRADE-06 Bảng điểm sinh viên, GPA học kỳ và CPA tích lũy
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpGet("sinh-vien/{sinhVienId:int}")]
    public async Task<ActionResult<ApiResponse<BangDiemSinhVienDto>>> GetBangDiem(int sinhVienId)
    {
        try
        {
            var result = await _service.GetBangDiemAsync(sinhVienId);
            return result is null
                ? NotFound(ApiResponse<BangDiemSinhVienDto>.Fail("Không tìm thấy sinh viên."))
                : Ok(ApiResponse<BangDiemSinhVienDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<BangDiemSinhVienDto>.Fail(ex.Message));
        }
    }

    // GRADE-07 Cảnh báo học vụ của một sinh viên trong học kỳ
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpGet("sinh-vien/{sinhVienId:int}/canh-bao")]
    public async Task<ActionResult<ApiResponse<CanhBaoHocVuDto>>> GetCanhBaoHocVu(
        int sinhVienId, [FromQuery] int hocKyId)
    {
        if (hocKyId <= 0) return BadRequest(ApiResponse<CanhBaoHocVuDto>.Fail("Mã học kỳ phải lớn hơn 0."));
        try
        {
            var result = await _service.GetCanhBaoHocVuAsync(sinhVienId, hocKyId);
            return result is null
                ? NotFound(ApiResponse<CanhBaoHocVuDto>.Fail("Không tìm thấy sinh viên hoặc học kỳ."))
                : Ok(ApiResponse<CanhBaoHocVuDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CanhBaoHocVuDto>.Fail(ex.Message));
        }
    }

    // GRADE-07 Danh sách cảnh báo đã đủ cơ sở đánh giá
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpGet("canh-bao")]
    public async Task<ActionResult<ApiResponse<PagedResult<CanhBaoHocVuDto>>>> GetCanhBaoHocVuPaged(
        [FromQuery] int hocKyId, [FromQuery] PagedQuery query)
    {
        if (hocKyId <= 0) return BadRequest(ApiResponse<PagedResult<CanhBaoHocVuDto>>.Fail("Mã học kỳ phải lớn hơn 0."));
        try
        {
            var result = await _service.GetCanhBaoHocVuPagedAsync(hocKyId, query);
            return result is null
                ? NotFound(ApiResponse<PagedResult<CanhBaoHocVuDto>>.Fail("Không tìm thấy học kỳ."))
                : Ok(ApiResponse<PagedResult<CanhBaoHocVuDto>>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PagedResult<CanhBaoHocVuDto>>.Fail(ex.Message));
        }
    }

    // GRADE-05 Quản lý yêu cầu phúc khảo và điều chỉnh điểm
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost("/api/phuc-khao")]
    public async Task<ActionResult<ApiResponse<PhucKhaoDto>>> CreatePhucKhao([FromBody] PhucKhaoRequest request)
    {
        try
        {
            var result = await _service.CreatePhucKhaoAsync(request);
            return result is null
                ? NotFound(ApiResponse<PhucKhaoDto>.Fail("Không tìm thấy bản ghi điểm."))
                : StatusCode(StatusCodes.Status201Created, ApiResponse<PhucKhaoDto>.Ok(result, "Đã tạo yêu cầu phúc khảo."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PhucKhaoDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PhucKhaoDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpGet("/api/phuc-khao")]
    public async Task<ActionResult<ApiResponse<PagedResult<PhucKhaoDto>>>> GetPhucKhaoPaged(
        [FromQuery] PagedQuery query, [FromQuery] string? trangThai)
    {
        try
        {
            return Ok(ApiResponse<PagedResult<PhucKhaoDto>>.Ok(
                await _service.GetPhucKhaoPagedAsync(query, trangThai)));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PagedResult<PhucKhaoDto>>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpGet("/api/phuc-khao/{id:int}")]
    public async Task<ActionResult<ApiResponse<PhucKhaoDto>>> GetPhucKhaoById(int id)
    {
        var result = await _service.GetPhucKhaoByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<PhucKhaoDto>.Fail("Không tìm thấy yêu cầu phúc khảo."))
            : Ok(ApiResponse<PhucKhaoDto>.Ok(result));
    }

    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("/api/phuc-khao/{id:int}/xu-ly")]
    public async Task<ActionResult<ApiResponse<PhucKhaoDto>>> XuLyPhucKhao(
        int id, [FromBody] XuLyPhucKhaoRequest request)
    {
        try
        {
            var result = await _service.XuLyPhucKhaoAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<PhucKhaoDto>.Fail("Không tìm thấy yêu cầu phúc khảo."))
                : Ok(ApiResponse<PhucKhaoDto>.Ok(result, "Đã xử lý yêu cầu phúc khảo."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PhucKhaoDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PhucKhaoDto>.Fail(ex.Message));
        }
    }
}
