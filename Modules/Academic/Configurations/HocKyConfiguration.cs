using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho HocKy.</summary>
public class HocKyConfiguration : IEntityTypeConfiguration<HocKy>
{
    public void Configure(EntityTypeBuilder<HocKy> builder)
    {
        builder.ToTable("HocKy");
        builder.Property(x => x.Ten).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.NamHocId, x.SoThuTu }).IsUnique();
        builder.HasOne<NamHoc>().WithMany().HasForeignKey(x => x.NamHocId).OnDelete(DeleteBehavior.Restrict);
    }
}
