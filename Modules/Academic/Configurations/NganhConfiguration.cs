using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho Nganh.</summary>
public class NganhConfiguration : IEntityTypeConfiguration<Nganh>
{
    public void Configure(EntityTypeBuilder<Nganh> builder)
    {
        builder.ToTable("Nganh");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<Khoa>().WithMany().HasForeignKey(x => x.KhoaId).OnDelete(DeleteBehavior.Restrict);
    }
}
