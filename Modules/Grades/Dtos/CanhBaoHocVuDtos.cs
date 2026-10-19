using System.Text.Json.Serialization;

namespace StudentManagement.Modules.Grades.Dtos;

/// <summary>Kết quả đánh giá cảnh báo học vụ (JSON trả về dạng chuỗi, ví dụ "CanhBao").</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiCanhBao>))]
public enum TrangThaiCanhBao
{
    /// <summary>Chưa đủ dữ liệu để kết luận (học kỳ chưa kết thúc, còn điểm chưa khóa...).</summary>
    ChuaDuDuLieu,
    CanhBao,
    KhongCanhBao
}

/// <summary>Đánh giá học vụ từ dữ liệu hiện tại; chưa phải quyết định của Phòng đào tạo.</summary>
public record CanhBaoHocVuDto(
    int SinhVienId,
    string MSSV,
    string HoTen,
    int HocKyId,
    string TenHocKy,
    string TenNamHoc,
    decimal? Gpa,
    decimal? Cpa,
    int TongTinChiDangKy,
    int SoHocPhanKhongDat,
    int SoTinChiKhongDat,
    TrangThaiCanhBao TrangThaiCanhBao,
    IReadOnlyList<string> DanhSachLyDo,
    string GhiChu);
