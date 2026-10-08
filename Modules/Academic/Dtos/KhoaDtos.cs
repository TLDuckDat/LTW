using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.Academic.Dtos;

/// <summary>Dữ liệu Khoa trả về cho client.</summary>
public record KhoaDto(int Id, string Ma, string Ten, string? MoTa, DateTime CreatedAt, DateTime? UpdatedAt);

/// <summary>Dữ liệu tạo mới / cập nhật Khoa.</summary>
public record KhoaRequest(
    [Required, MaxLength(20)] string Ma,
    [Required, MaxLength(200)] string Ten,
    [MaxLength(1000)] string? MoTa);
