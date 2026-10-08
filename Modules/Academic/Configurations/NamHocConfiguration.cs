using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho NamHoc.</summary>
public class NamHocConfiguration : IEntityTypeConfiguration<NamHoc>
{
    public void Configure(EntityTypeBuilder<NamHoc> builder)
    {
        builder.ToTable("NamHoc");
        builder.Property(x => x.Ten).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Ten).IsUnique();
    }
}
