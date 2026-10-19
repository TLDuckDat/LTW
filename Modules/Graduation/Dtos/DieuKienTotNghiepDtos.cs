using System.Text.Json.Serialization;

namespace StudentManagement.Modules.Graduation.Dtos;

/// <summary>Kết quả kiểm tra một điều kiện tốt nghiệp.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TrangThaiDieuKien>))]
public enum TrangThaiDieuKien
{
    Dat,
    ChuaDat,
    ChuaXacDinh
}

/// <summary>Kết luận chung sau khi kiểm tra tất cả điều kiện tốt nghiệp.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<KetLuanTotNghiep>))]
public enum KetLuanTotNghiep
{
    DuDieuKien,
    KhongDuDieuKien,
    ChuaTheKetLuan
}

public record DieuKienDto(string Ma, string Ten, TrangThaiDieuKien TrangThai, string GiaiThich);

public record ThucTapTotNghiepDto(bool? BatBuoc, int SoBanGhi,
    decimal? Diem, decimal? DiemToiThieu, bool DaCoKetQua);

public record KhoaLuanTotNghiepDto(bool? BatBuoc, int? DeTaiId,
    int? HoiDongBaoVeId, decimal? Diem, decimal? DiemToiThieu, bool DaCoKetQua);

/// <summary>Môn bắt buộc của chương trình đào tạo: đã đạt khi có điểm đã khóa từ mức đạt trở lên.</summary>
public record MonBatBuocTotNghiepDto(int SoMonBatBuoc, int SoMonDaDat, IReadOnlyList<string> MonConThieu);

/// <summary>XepLoaiTotNghiep: xếp loại dự kiến theo CPA hiện tại (null khi chưa có CPA).</summary>
public record DieuKienTotNghiepDto(
    int SinhVienId, string MaSinhVien, string HoTen, string TrangThaiSinhVien,
    int? ChuongTrinhDaoTaoId, string? TenChuongTrinhDaoTao,
    int? TongTinChiYeuCau, int? TinChiTichLuy, int? TinChiConThieu,
    decimal? Cpa, decimal? CpaToiThieu, string? XepLoaiTotNghiep,
    ThucTapTotNghiepDto ThucTap, KhoaLuanTotNghiepDto KhoaLuan, MonBatBuocTotNghiepDto MonBatBuoc,
    IReadOnlyList<DieuKienDto> DanhSachDieuKien, KetLuanTotNghiep KetLuan);
