using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Configurations;

/// <summary>Cấu hình EF cho HocPhi.</summary>
public class HocPhiConfiguration : IEntityTypeConfiguration<HocPhi>
{
    public void Configure(EntityTypeBuilder<HocPhi> builder)
    {
        builder.ToTable("HocPhi");
        builder.Property(x => x.SoTien).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.SinhVienId, x.HocKyId }).IsUnique();
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
    }
}
