using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Auth.Entities;

namespace StudentManagement.Modules.Auth.Configurations;

public class NhatKyHoatDongConfiguration : IEntityTypeConfiguration<NhatKyHoatDong>
{
    public void Configure(EntityTypeBuilder<NhatKyHoatDong> builder)
    {
        builder.ToTable("NhatKyHoatDong");
        builder.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
        builder.Property(x => x.HanhDong).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PhuongThuc).HasMaxLength(10);
        builder.Property(x => x.DuongDan).HasMaxLength(300);
        builder.Property(x => x.ChiTiet).HasMaxLength(1000);
        builder.Property(x => x.DiaChiIp).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.HasIndex(x => x.CreatedAt);
        // Đếm số lần đăng nhập sai gần đây theo tên đăng nhập
        builder.HasIndex(x => new { x.TenDangNhap, x.HanhDong, x.CreatedAt });
        builder.HasIndex(x => new { x.TaiKhoanId, x.CreatedAt });
        builder.HasOne<TaiKhoan>()
            .WithMany()
            .HasForeignKey(x => x.TaiKhoanId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
