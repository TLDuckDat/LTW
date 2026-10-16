using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Courses.Dtos;

/// <summary>Dữ liệu môn học trả về cho client.</summary>
public record MonHocDto(int Id, string Ma, string Ten, int SoTinChi, DateTime CreatedAt, DateTime? UpdatedAt);

/// <summary>Dữ liệu tạo mới / cập nhật môn học.</summary>
public record MonHocRequest(
    [Required, MaxLength(20)] string Ma,
    [Required, MaxLength(200)] string Ten,
    [Range(1, 20)] int SoTinChi);

/// <summary>Thêm một môn tiên quyết cho môn học.</summary>
public record TienQuyetRequest([Range(1, int.MaxValue)] int TienQuyetId);
