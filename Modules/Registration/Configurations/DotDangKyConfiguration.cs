using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Registration.Configurations;

/// <summary>Cấu hình EF cho DotDangKy.</summary>
public class DotDangKyConfiguration : IEntityTypeConfiguration<DotDangKy>
{
    public void Configure(EntityTypeBuilder<DotDangKy> builder)
    {
        builder.ToTable("DotDangKy");
        builder.Property(x => x.Ten).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(30);
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
    }
}
