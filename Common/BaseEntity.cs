namespace StudentManagement.Common;

/// <summary>Lớp cơ sở cho mọi thực thể: khóa chính và thời điểm tạo/cập nhật.</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
