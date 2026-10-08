using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Configurations;

/// <summary>Cấu hình EF cho XetTotNghiep.</summary>
public class XetTotNghiepConfiguration : IEntityTypeConfiguration<XetTotNghiep>
{
    public void Configure(EntityTypeBuilder<XetTotNghiep> builder)
    {
        builder.ToTable("XetTotNghiep");
        builder.Property(x => x.DiemTrungBinhTichLuy).HasPrecision(4, 2);
        builder.Property(x => x.XepLoai).HasMaxLength(30);
        builder.Property(x => x.KetQua).HasConversion<string>().HasMaxLength(30);
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
    }
}
