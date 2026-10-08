using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho ChuyenNganh.</summary>
public class ChuyenNganhConfiguration : IEntityTypeConfiguration<ChuyenNganh>
{
    public void Configure(EntityTypeBuilder<ChuyenNganh> builder)
    {
        builder.ToTable("ChuyenNganh");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<Nganh>().WithMany().HasForeignKey(x => x.NganhId).OnDelete(DeleteBehavior.Restrict);
    }
}
