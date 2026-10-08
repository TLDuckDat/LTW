using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Courses.Configurations;

/// <summary>Cấu hình EF cho LopHocPhan.</summary>
public class LopHocPhanConfiguration : IEntityTypeConfiguration<LopHocPhan>
{
    public void Configure(EntityTypeBuilder<LopHocPhan> builder)
    {
        builder.ToTable("LopHocPhan");
        builder.Property(x => x.Ma).HasMaxLength(30).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MonHoc>().WithMany().HasForeignKey(x => x.MonHocId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.GiangVienId).OnDelete(DeleteBehavior.Restrict);
    }
}
