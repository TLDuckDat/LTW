using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho Khoa.</summary>
public class KhoaConfiguration : IEntityTypeConfiguration<Khoa>
{
    public void Configure(EntityTypeBuilder<Khoa> builder)
    {
        builder.ToTable("Khoa");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MoTa).HasMaxLength(1000);
        builder.HasIndex(x => x.Ma).IsUnique();
    }
}
