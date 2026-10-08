using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Configurations;

/// <summary>Cấu hình EF cho PhongHoc.</summary>
public class PhongHocConfiguration : IEntityTypeConfiguration<PhongHoc>
{
    public void Configure(EntityTypeBuilder<PhongHoc> builder)
    {
        builder.ToTable("PhongHoc");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
    }
}
