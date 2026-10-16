
using StudentManagement.Modules.StudentAffairs.Entities;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Modules.StudentAffairs.Dtos;

// SA-01: Dữ liệu nhập điểm rèn luyện.
public class DiemRenLuyenRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "SinhVienId phải lớn hơn 0.")]
    public int SinhVienId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "HocKyId phải lớn hơn 0.")]
    public int HocKyId { get; set; }

    [Range(0, 100, ErrorMessage = "Điểm rèn luyện phải từ 0 đến 100.")]
    public int Diem { get; set; }
}

// SA-05: Dữ liệu xét học bổng.
public record KhenThuongKyLuatRequest(
    int SinhVienId,
    LoaiKhenThuongKyLuat Loai,
    string? SoQuyetDinh,
    DateOnly NgayQuyetDinh,
    string NoiDung
);

// SA-04: Dữ liệu tạo miễn giảm học phí.
public record MienGiamRequest(
    int SinhVienId,
    int HocKyId,
    decimal TyLe,
    string? LyDo
);
