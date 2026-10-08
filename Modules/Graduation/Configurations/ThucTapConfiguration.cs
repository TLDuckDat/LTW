using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Configurations;

/// <summary>Cấu hình EF cho ThucTap.</summary>
public class ThucTapConfiguration : IEntityTypeConfiguration<ThucTap>
{
    public void Configure(EntityTypeBuilder<ThucTap> builder)
    {
        builder.ToTable("ThucTap");
        builder.Property(x => x.DonViThucTap).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Diem).HasPrecision(4, 2);
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.GiangVienHuongDanId).OnDelete(DeleteBehavior.Restrict);
    }
}
