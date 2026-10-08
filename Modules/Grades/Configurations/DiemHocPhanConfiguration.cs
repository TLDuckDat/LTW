using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Grades.Entities;
using StudentManagement.Modules.Registration.Entities;

namespace StudentManagement.Modules.Grades.Configurations;

/// <summary>Cấu hình EF cho DiemHocPhan.</summary>
public class DiemHocPhanConfiguration : IEntityTypeConfiguration<DiemHocPhan>
{
    public void Configure(EntityTypeBuilder<DiemHocPhan> builder)
    {
        builder.ToTable("DiemHocPhan");
        builder.Property(x => x.DiemQuaTrinh).HasPrecision(4, 2);
        builder.Property(x => x.DiemGiuaKy).HasPrecision(4, 2);
        builder.Property(x => x.DiemCuoiKy).HasPrecision(4, 2);
        builder.Property(x => x.DiemTongKet).HasPrecision(4, 2);
        builder.Property(x => x.DiemChu).HasMaxLength(2);
        builder.HasIndex(x => x.DangKyHocPhanId).IsUnique();
        builder.HasOne<DangKyHocPhan>().WithMany().HasForeignKey(x => x.DangKyHocPhanId).OnDelete(DeleteBehavior.Restrict);
    }
}
