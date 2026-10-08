using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Auth.Entities;

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
    }
}
