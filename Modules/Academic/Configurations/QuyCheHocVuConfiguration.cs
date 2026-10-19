using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho QuyCheHocVu.</summary>
public class QuyCheHocVuConfiguration : IEntityTypeConfiguration<QuyCheHocVu>
{
    public void Configure(EntityTypeBuilder<QuyCheHocVu> builder)
    {
        builder.ToTable("QuyCheHocVu");
        builder.Property(x => x.Ma).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GiaTri).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DonVi).HasMaxLength(50);
        builder.Property(x => x.MoTa).HasMaxLength(500);

        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasIndex(x => x.ApDungTuKhoa);
    }
}
