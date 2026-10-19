using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Configurations;

/// <summary>Cấu hình EF cho DeTaiKhoaLuan.</summary>
public class DeTaiKhoaLuanConfiguration : IEntityTypeConfiguration<DeTaiKhoaLuan>
{
    public void Configure(EntityTypeBuilder<DeTaiKhoaLuan> builder)
    {
        builder.ToTable("DeTaiKhoaLuan");
        builder.Property(x => x.TenDeTai).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.SinhVienId).IsUnique().HasFilter("[SinhVienId] IS NOT NULL");
        builder.ToTable(t => t.HasCheckConstraint("CK_DeTaiKhoaLuan_TenDeTai", "LEN(TRIM([TenDeTai])) > 0"));
        builder.Property(x => x.Diem).HasPrecision(4, 2);
        builder.Property(x => x.NhanXetBaoVe).HasMaxLength(1000);
        builder.ToTable(t => t.HasCheckConstraint("CK_DeTaiKhoaLuan_Diem", "[Diem] IS NULL OR ([Diem] >= 0 AND [Diem] <= 10)"));
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.GiangVienHuongDanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<HoiDongBaoVe>().WithMany().HasForeignKey(x => x.HoiDongBaoVeId).OnDelete(DeleteBehavior.Restrict);
    }
}
