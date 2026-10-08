using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Configurations;

/// <summary>Cấu hình EF cho MienGiam.</summary>
public class MienGiamConfiguration : IEntityTypeConfiguration<MienGiam>
{
    public void Configure(EntityTypeBuilder<MienGiam> builder)
    {
        builder.ToTable("MienGiam");
        builder.Property(x => x.TyLe).HasPrecision(5, 2);
        builder.Property(x => x.LyDo).HasMaxLength(500);
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
    }
}
