using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Configurations;

/// <summary>Cấu hình EF cho MonHoc.</summary>
public class MonHocConfiguration : IEntityTypeConfiguration<MonHoc>
{
    public void Configure(EntityTypeBuilder<MonHoc> builder)
    {
        builder.ToTable("MonHoc");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
    }
}
