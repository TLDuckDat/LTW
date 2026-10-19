using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;
using StudentManagement.Data;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Services;

/// <summary>
/// Mã quy chế (bảng QuyCheHocVu, cột Ma) mà các module đọc làm tham số nghiệp vụ.
/// Chưa cấu hình thì module dùng giá trị mặc định của mình (ghi trong ngoặc).
/// GiaTri ghi theo dạng số invariant, ví dụ "600000" hoặc "2.5"; chỉ đọc quy chế áp dụng chung (ApDungTuKhoa = null).
/// </summary>
public static class MaQuyChe
{
    // Registration
    /// <summary>Số tín chỉ tối đa được đăng ký trong một học kỳ (24).</summary>
    public const string TinChiToiDaHocKy = "TIN_CHI_TOI_DA_HOC_KY";

    // Grades - cảnh báo học vụ
    /// <summary>GPA tối thiểu ở học kỳ 1 của năm học đầu tiên (0.80).</summary>
    public const string CanhBaoGpaHocKyDauTien = "CB_GPA_HOC_KY_DAU_TIEN";
    /// <summary>GPA tối thiểu ở các học kỳ chính khác (1.00).</summary>
    public const string CanhBaoGpaToiThieu = "CB_GPA_TOI_THIEU";
    /// <summary>Số tín chỉ tối thiểu phải đăng ký mỗi học kỳ chính (12).</summary>
    public const string CanhBaoTinChiToiThieu = "CB_TIN_CHI_TOI_THIEU";

    // StudentAffairs
    /// <summary>Đơn giá một tín chỉ, VND (600000).</summary>
    public const string DonGiaTinChi = "DON_GIA_TIN_CHI";
    /// <summary>Điểm rèn luyện tối thiểu để xét học bổng (80).</summary>
    public const string HocBongDiemRenLuyenToiThieu = "HB_DIEM_REN_LUYEN_TOI_THIEU";
    /// <summary>GPA học kỳ tối thiểu để xét học bổng (2.5).</summary>
    public const string HocBongGpaToiThieu = "HB_GPA_TOI_THIEU";
    /// <summary>Số tiền học bổng khuyến khích, VND (1000000).</summary>
    public const string HocBongSoTien = "HB_SO_TIEN";
}

public static class QuyCheThamSo
{
    /// <summary>Đọc tham số dạng số từ quy chế; chưa cấu hình thì trả về <paramref name="macDinh"/>.</summary>
    public static async Task<decimal> GetThamSoAsync(this AppDbContext db, string ma, decimal macDinh)
    {
        var giaTri = await db.Set<QuyCheHocVu>().AsNoTracking()
            .Where(x => x.Ma == ma && x.ApDungTuKhoa == null)
            .Select(x => x.GiaTri)
            .FirstOrDefaultAsync();
        if (giaTri is null) return macDinh;

        return decimal.TryParse(giaTri.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var soLieu)
            ? soLieu
            : throw AppException.Conflict($"Quy chế '{ma}' có giá trị '{giaTri}' không phải là số hợp lệ.");
    }

    /// <summary>Như trên cho tham số số nguyên không âm (số tín chỉ, điểm rèn luyện...).</summary>
    public static async Task<int> GetThamSoNguyenAsync(this AppDbContext db, string ma, int macDinh)
    {
        var giaTri = await db.GetThamSoAsync(ma, macDinh);
        if (giaTri < 0 || giaTri > int.MaxValue || decimal.Truncate(giaTri) != giaTri)
            throw AppException.Conflict($"Quy chế '{ma}' phải là số nguyên không âm (đang là {giaTri}).");
        return (int)giaTri;
    }
}
