using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Auth.Configurations;

/// <summary>Cấu hình EF cho TaiKhoan.</summary>
public class TaiKhoanConfiguration : IEntityTypeConfiguration<TaiKhoan>
{
    public void Configure(EntityTypeBuilder<TaiKhoan> builder)
    {
        builder.ToTable("TaiKhoan");
        builder.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MatKhauHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
        builder.Property(x => x.VaiTro).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.TenDangNhap).IsUnique();
        builder.HasIndex(x => x.SinhVienId).IsUnique();
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.GiangVienId).IsUnique();
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.GiangVienId).OnDelete(DeleteBehavior.Restrict);
    }
}
