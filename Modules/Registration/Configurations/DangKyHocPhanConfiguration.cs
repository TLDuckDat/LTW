using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Courses.Entities;
using StudentManagement.Modules.Registration.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Registration.Configurations;

/// <summary>Cấu hình EF cho DangKyHocPhan.</summary>
public class DangKyHocPhanConfiguration : IEntityTypeConfiguration<DangKyHocPhan>
{
    public void Configure(EntityTypeBuilder<DangKyHocPhan> builder)
    {
        builder.ToTable("DangKyHocPhan");
        builder.Property(x => x.LoaiDangKy).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.SinhVienId, x.LopHocPhanId }).IsUnique();
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<LopHocPhan>().WithMany().HasForeignKey(x => x.LopHocPhanId).OnDelete(DeleteBehavior.Restrict);
    }
}
