using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Courses.Dtos;

/// <summary>Dữ liệu phòng học trả về cho client.</summary>
public record PhongHocDto(int Id, string Ma, int SucChua, DateTime CreatedAt, DateTime? UpdatedAt);

/// <summary>Dữ liệu tạo mới / cập nhật phòng học.</summary>
public record PhongHocRequest(
    [Required, MaxLength(20)] string Ma,
    [Range(1, 1000)] int SucChua);
