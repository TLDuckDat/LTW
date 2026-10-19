using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Auth.Entities;

namespace StudentManagement.Modules.Auth.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.LyDoThuHoi).HasMaxLength(30);
        builder.Property(x => x.DiaChiIp).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.TaiKhoanId, x.HetHan });
        builder.HasOne<TaiKhoan>().WithMany().HasForeignKey(x => x.TaiKhoanId).OnDelete(DeleteBehavior.Cascade);
    }
}
