using System.ComponentModel.DataAnnotations;
using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Dtos;

public record NamHocDto(
    int Id,
    string Ten,
    DateOnly NgayBatDau,
    DateOnly NgayKetThuc,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record NamHocRequest(
    [Required(ErrorMessage = "Tên năm học là bắt buộc (ví dụ: 2025-2026).")]
    [MaxLength(20, ErrorMessage = "Tên năm học tối đa 20 ký tự.")]
    string Ten,

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    DateOnly NgayBatDau,

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    DateOnly NgayKetThuc
);
