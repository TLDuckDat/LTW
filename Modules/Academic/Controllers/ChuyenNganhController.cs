using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Common;
using StudentManagement.Modules.Academic.Dtos;
using StudentManagement.Modules.Academic.Services;

namespace StudentManagement.Modules.Academic.Controllers;

[ApiController]
[Route("api/chuyen-nganh")]
[Tags("Academic")]
public class ChuyenNganhController(IChuyenNganhService service) : ControllerBase
{
    private readonly IChuyenNganhService _service = service;

    // EDU-02 Danh sách chuyên ngành (phân trang, lọc theo ngành, tìm kiếm)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ChuyenNganhDto>>>> GetPaged([FromQuery] ChuyenNganhFilterQuery query)
        => Ok(ApiResponse<PagedResult<ChuyenNganhDto>>.Ok(await _service.GetPagedAsync(query)));

    // EDU-02 Chi tiết chuyên ngành
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ChuyenNganhDto>>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null
            ? NotFound(ApiResponse<ChuyenNganhDto>.Fail("Không tìm thấy chuyên ngành."))
            : Ok(ApiResponse<ChuyenNganhDto>.Ok(result));
    }

    // EDU-02 Thêm chuyên ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChuyenNganhDto>>> Create([FromBody] ChuyenNganhRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ChuyenNganhDto>.Ok(result, "Đã tạo chuyên ngành."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ChuyenNganhDto>.Fail(ex.Message));
        }
    }

    // EDU-02 Cập nhật chuyên ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ChuyenNganhDto>>> Update(int id, [FromBody] ChuyenNganhRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result is null
                ? NotFound(ApiResponse<ChuyenNganhDto>.Fail("Không tìm thấy chuyên ngành."))
                : Ok(ApiResponse<ChuyenNganhDto>.Ok(result, "Đã cập nhật chuyên ngành."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<ChuyenNganhDto>.Fail(ex.Message));
        }
    }

    // EDU-02 Xóa chuyên ngành
    [Authorize(Roles = "Admin,PhongDaoTao")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(ApiResponse<object>.Ok(null, "Đã xóa chuyên ngành."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy chuyên ngành."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
