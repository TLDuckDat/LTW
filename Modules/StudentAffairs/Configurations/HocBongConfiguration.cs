using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Configurations;

/// <summary>Cấu hình EF cho HocBong.</summary>
public class HocBongConfiguration : IEntityTypeConfiguration<HocBong>
{
    public void Configure(EntityTypeBuilder<HocBong> builder)
    {
        builder.ToTable("HocBong");
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SoTien).HasPrecision(18, 2);
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
    }
}
