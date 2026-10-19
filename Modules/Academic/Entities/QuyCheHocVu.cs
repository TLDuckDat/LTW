using StudentManagement.Common;

namespace StudentManagement.Modules.Academic.Entities;

/// <summary>Quy chế và điều kiện học vụ.</summary>
public class QuyCheHocVu : BaseEntity
{
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string GiaTri { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public string? MoTa { get; set; }
    public int? ApDungTuKhoa { get; set; }
}
