using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Configurations;

/// <summary>Cấu hình EF cho GiangVien.</summary>
public class GiangVienConfiguration : IEntityTypeConfiguration<GiangVien>
{
    public void Configure(EntityTypeBuilder<GiangVien> builder)
    {
        builder.ToTable("GiangVien");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
        builder.Property(x => x.HocVi).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.SoDienThoai).HasMaxLength(20);
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<BoMon>().WithMany().HasForeignKey(x => x.BoMonId).OnDelete(DeleteBehavior.Restrict);
    }
}
