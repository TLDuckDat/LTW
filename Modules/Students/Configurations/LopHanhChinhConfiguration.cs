using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Configurations;

/// <summary>Cấu hình EF cho LopHanhChinh.</summary>
public class LopHanhChinhConfiguration : IEntityTypeConfiguration<LopHanhChinh>
{
    public void Configure(EntityTypeBuilder<LopHanhChinh> builder)
    {
        builder.ToTable("LopHanhChinh");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<Nganh>().WithMany().HasForeignKey(x => x.NganhId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.CoVanId).OnDelete(DeleteBehavior.Restrict);
    }
}
