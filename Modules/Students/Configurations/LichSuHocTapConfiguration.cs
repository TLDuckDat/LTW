using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Students.Configurations;

/// <summary>Cấu hình EF cho LichSuHocTap.</summary>
public class LichSuHocTapConfiguration : IEntityTypeConfiguration<LichSuHocTap>
{
    public void Configure(EntityTypeBuilder<LichSuHocTap> builder)
    {
        builder.ToTable("LichSuHocTap");
        builder.Property(x => x.TrangThaiCu).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TrangThaiMoi).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.SoQuyetDinh).HasMaxLength(50);
        builder.Property(x => x.LyDo).HasMaxLength(500).IsRequired();
        builder.Property(x => x.GhiChu).HasMaxLength(500);

        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Cascade);
    }
}
