using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Configurations;

/// <summary>Cấu hình EF cho SinhVien.</summary>
public class SinhVienConfiguration : IEntityTypeConfiguration<SinhVien>
{
    public void Configure(EntityTypeBuilder<SinhVien> builder)
    {
        builder.ToTable("SinhVien");
        builder.Property(x => x.MSSV).HasMaxLength(20).IsRequired();
        builder.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
        builder.Property(x => x.GioiTinh).HasMaxLength(10);
        builder.Property(x => x.CCCD).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.SoDienThoai).HasMaxLength(20);
        builder.Property(x => x.DiaChi).HasMaxLength(250);
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(x => x.MSSV).IsUnique();
        builder.HasIndex(x => x.CCCD);

        builder.HasOne<Nganh>().WithMany().HasForeignKey(x => x.NganhId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ChuyenNganh>().WithMany().HasForeignKey(x => x.ChuyenNganhId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LopHanhChinh>().WithMany().HasForeignKey(x => x.LopHanhChinhId).OnDelete(DeleteBehavior.Restrict);
    }
}
