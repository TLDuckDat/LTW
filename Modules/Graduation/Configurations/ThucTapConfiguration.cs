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
        builder.Property(x => x.DonViThucTap).HasMaxLength(200);
        builder.Property(x => x.Diem).HasPrecision(4, 2);
        builder.Property(x => x.NhanXet).HasMaxLength(1000);
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(x => new { x.SinhVienId, x.HocKyId }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("CK_ThucTap_KetQua",
            "([Diem] IS NULL AND [TrangThai] <> 'HoanThanh') OR ([Diem] BETWEEN 0 AND 10 AND [TrangThai] = 'HoanThanh')"));
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.GiangVienHuongDanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CoSoThucTap>().WithMany().HasForeignKey(x => x.CoSoThucTapId).OnDelete(DeleteBehavior.Restrict);
    }
}
