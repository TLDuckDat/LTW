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
        builder.Property(x => x.HanhDong).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DiaChiIp).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.TenDangNhap);
        builder.HasOne<TaiKhoan>()
            .WithMany()
            .HasForeignKey(x => x.TaiKhoanId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
