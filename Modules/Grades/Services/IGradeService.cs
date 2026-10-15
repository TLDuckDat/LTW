using StudentManagement.Common;
using StudentManagement.Modules.Grades.Dtos;

namespace StudentManagement.Modules.Grades.Services;

/// <summary>Nghiệp vụ module Grades (Epic GRD).</summary>
public interface IGradeService
{
    // GRADE-01 Danh sách sinh viên của lớp học phần (null nếu không có lớp)
    Task<PagedResult<SinhVienTrongLopDto>?> GetDanhSachSinhVienAsync(int lopHocPhanId, PagedQuery query);
    // GRADE-01 Thêm sinh viên vào lớp học phần (null nếu không có lớp hoặc sinh viên)
    Task<DangKyHocPhanDto?> ThemSinhVienAsync(int lopHocPhanId, ThemSinhVienRequest request);
    // GRADE-01 Chuyển lượt đăng ký sang lớp học phần khác cùng môn (null nếu không tìm thấy)
    Task<DangKyHocPhanDto?> ChuyenLopAsync(int lopHocPhanId, int dangKyHocPhanId, ChuyenLopRequest request);
    // GRADE-01 Hủy lượt đăng ký trong lớp, không xóa vật lý (false nếu không tìm thấy)
    Task<bool> HuyDangKyAsync(int lopHocPhanId, int dangKyHocPhanId);

    // GRADE-02 Nhập và cập nhật điểm theo lớp học phần (null nếu không có lớp)
    Task<NhapDiemResultDto?> NhapDiemAsync(int lopHocPhanId, NhapDiemRequest request);
    // GRADE-03 Tính điểm tổng kết, điểm chữ
    Task<TinhDiemResultDto?> TinhDiemAsync(int lopHocPhanId);
    // GRADE-04 Duyệt và khóa bảng điểm
    Task<KhoaDiemResultDto?> KhoaDiemAsync(int lopHocPhanId);
    // GRADE-06 Bảng điểm sinh viên, GPA học kỳ và CPA tích lũy
    Task<BangDiemSinhVienDto?> GetBangDiemAsync(int sinhVienId);
    // GRADE-07 Đánh giá cảnh báo học vụ theo học kỳ chính (null nếu sinh viên/học kỳ không tồn tại)
    Task<CanhBaoHocVuDto?> GetCanhBaoHocVuAsync(int sinhVienId, int hocKyId);
    Task<PagedResult<CanhBaoHocVuDto>?> GetCanhBaoHocVuPagedAsync(int hocKyId, PagedQuery query);
    // GRADE-05 Quản lý yêu cầu phúc khảo và điều chỉnh điểm
    Task<PhucKhaoDto?> CreatePhucKhaoAsync(PhucKhaoRequest request);
    Task<PagedResult<PhucKhaoDto>> GetPhucKhaoPagedAsync(PagedQuery query, string? trangThai);
    Task<PhucKhaoDto?> GetPhucKhaoByIdAsync(int id);
    Task<PhucKhaoDto?> XuLyPhucKhaoAsync(int id, XuLyPhucKhaoRequest request);
}
