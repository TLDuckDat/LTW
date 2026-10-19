using System.Text.Json.Serialization;

namespace StudentManagement.Modules.StudentAffairs.Entities;

/// <summary>Phân loại khen thưởng / kỷ luật (JSON dạng chuỗi; vẫn nhận số 0/1 khi gửi lên).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<LoaiKhenThuongKyLuat>))]
public enum LoaiKhenThuongKyLuat
{
    KhenThuong,
    KyLuat
}
