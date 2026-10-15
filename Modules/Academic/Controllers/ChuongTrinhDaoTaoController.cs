using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/chuong-trinh-dao-tao")]
[Tags("Academic")]
public class ChuongTrinhDaoTaoController(IChuongTrinhDaoTaoService service) : ControllerBase
{
    private readonly IChuongTrinhDaoTaoService _service = service;

    // ==========================================
    // EDU-04: QUẢN LÝ CHƯƠNG TRÌNH ĐÀO TẠO
    // ==========================================

    // Danh sách CTĐT (lọc theo ngành, khóa tuyển, tìm kiếm)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ChuongTrinhDaoTaoDto>>>> GetPaged([FromQuery] ChuongTrinhDaoTaoFilterQuery query)
        => Ok(ApiResponse<PagedResult<ChuongTrinhDaoTaoDto>>.Ok(await _service.GetPagedAsync(query)));

    // Chi tiết CTĐT (kèm danh sách môn học)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ChuongTrinhDaoTaoDetailDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<ChuongTrinhDaoTaoDetailDto>.Fail("Không tìm thấy chương trình đào tạo."))
            : Ok(ApiResponse<ChuongTrinhDaoTaoDetailDto>.Ok(result));
    }

    // Thêm CTĐT
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChuongTrinhDaoTaoDto>>> Create([FromBody] ChuongTrinhDaoTaoRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ChuongTrinhDaoTaoDto>.Ok(result, "Đã tạo chương trình đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ChuongTrinhDaoTaoDto>.Fail(ex.Message));
        }
    }

    // Cập nhật CTĐT
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ChuongTrinhDaoTaoDto>>> Update(int id, [FromBody] ChuongTrinhDaoTaoRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<ChuongTrinhDaoTaoDto>.Fail("Không tìm thấy chương trình đào tạo."))
                : Ok(ApiResponse<ChuongTrinhDaoTaoDto>.Ok(result, "Đã cập nhật chương trình đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ChuongTrinhDaoTaoDto>.Fail(ex.Message));
        }
    }

    // Xóa CTĐT
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa chương trình đào tạo."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy chương trình đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ==========================================
    // EDU-05: QUẢN LÝ MÔN HỌC TRONG CTĐT
    // ==========================================

    // Danh sách môn học trong CTĐT
    [HttpGet("{id:int}/mon-hoc")]
    public async Task<ActionResult<ApiResponse<List<MonHocTrongCtdtDto>>>> GetDanhSachMonHoc(int id, [FromQuery] int? hocKyGoiY)
        => Ok(ApiResponse<List<MonHocTrongCtdtDto>>.Ok(await _service.GetDanhSachMonHocAsync(id, hocKyGoiY)));

    // Thêm môn học vào CTĐT
    [HttpPost("{id:int}/mon-hoc")]
    public async Task<ActionResult<ApiResponse<MonHocTrongCtdtDto>>> ThemMonHoc(int id, [FromBody] ThemMonHocVaoCtdtRequest request)
    {
        try
        {
            var result = await _service.ThemMonHocAsync(id, request);
            return Ok(ApiResponse<MonHocTrongCtdtDto>.Ok(result, "Đã thêm môn học vào chương trình đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<MonHocTrongCtdtDto>.Fail(ex.Message));
        }
    }

    // Cập nhật thông tin môn học trong CTĐT (đổi kỳ gợi ý, đổi bắt buộc/tự chọn)
    [HttpPut("{id:int}/mon-hoc/{monHocId:int}")]
    public async Task<ActionResult<ApiResponse<MonHocTrongCtdtDto>>> CapNhatMonHoc(int id, int monHocId, [FromBody] CapNhatMonHocTrongCtdtRequest request)
    {
        try
        {
            var result = await _service.CapNhatMonHocAsync(id, monHocId, request);
            return result is null
                ? NotFound(ApiResponse<MonHocTrongCtdtDto>.Fail("Không tìm thấy môn học trong chương trình đào tạo này."))
                : Ok(ApiResponse<MonHocTrongCtdtDto>.Ok(result, "Đã cập nhật môn học trong chương trình đào tạo."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<MonHocTrongCtdtDto>.Fail(ex.Message));
        }
    }

    // Gỡ môn học khỏi CTĐT
    [HttpDelete("{id:int}/mon-hoc/{monHocId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> XoaMonHoc(int id, int monHocId)
    {
        return await _service.XoaMonHocAsync(id, monHocId)
            ? Ok(ApiResponse<object>.Ok(null, "Đã gỡ môn học khỏi chương trình đào tạo."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy môn học trong chương trình đào tạo này."));
    }
}
