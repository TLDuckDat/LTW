using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho BoMon.</summary>
public class BoMonConfiguration : IEntityTypeConfiguration<BoMon>
{
    public void Configure(EntityTypeBuilder<BoMon> builder)
    {
        builder.ToTable("BoMon");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<Khoa>().WithMany().HasForeignKey(x => x.KhoaId).OnDelete(DeleteBehavior.Restrict);
    }
}
