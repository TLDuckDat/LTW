using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Grades.Entities;

namespace StudentManagement.Modules.Grades.Configurations;

/// <summary>Cấu hình EF cho PhucKhao.</summary>
public class PhucKhaoConfiguration : IEntityTypeConfiguration<PhucKhao>
{
    public void Configure(EntityTypeBuilder<PhucKhao> builder)
    {
        builder.ToTable("PhucKhao");
        builder.Property(x => x.LyDo).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DiemCu).HasPrecision(4, 2);
        builder.Property(x => x.DiemMoi).HasPrecision(4, 2);
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(30);
        builder.HasOne<DiemHocPhan>().WithMany().HasForeignKey(x => x.DiemHocPhanId).OnDelete(DeleteBehavior.Restrict);
    }
}
