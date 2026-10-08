using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Configurations;

/// <summary>Cấu hình EF cho MonHocTienQuyet.</summary>
public class MonHocTienQuyetConfiguration : IEntityTypeConfiguration<MonHocTienQuyet>
{
    public void Configure(EntityTypeBuilder<MonHocTienQuyet> builder)
    {
        builder.ToTable("MonHocTienQuyet");
        builder.HasIndex(x => new { x.MonHocId, x.TienQuyetId }).IsUnique();
        builder.HasOne<MonHoc>().WithMany().HasForeignKey(x => x.MonHocId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MonHoc>().WithMany().HasForeignKey(x => x.TienQuyetId).OnDelete(DeleteBehavior.Restrict);
    }
}
