using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.Reports.Entities;

namespace StudentManagement.Modules.Reports.Configurations;

/// <summary>Cấu hình EF cho ThongBao.</summary>
public class ThongBaoConfiguration : IEntityTypeConfiguration<ThongBao>
{
    public void Configure(EntityTypeBuilder<ThongBao> builder)
    {
        builder.ToTable("ThongBao");
        builder.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NoiDung).HasMaxLength(4000).IsRequired();
        builder.HasOne<TaiKhoan>().WithMany().HasForeignKey(x => x.NguoiTaoId).OnDelete(DeleteBehavior.Restrict);
    }
}
