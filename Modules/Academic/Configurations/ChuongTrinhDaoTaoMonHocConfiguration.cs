using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho ChuongTrinhDaoTaoMonHoc.</summary>
public class ChuongTrinhDaoTaoMonHocConfiguration : IEntityTypeConfiguration<ChuongTrinhDaoTaoMonHoc>
{
    public void Configure(EntityTypeBuilder<ChuongTrinhDaoTaoMonHoc> builder)
    {
        builder.ToTable("ChuongTrinhDaoTaoMonHoc");
        builder.Property(x => x.GhiChu).HasMaxLength(500);

        builder.HasIndex(x => new { x.ChuongTrinhDaoTaoId, x.MonHocId }).IsUnique();
        builder.HasIndex(x => new { x.ChuongTrinhDaoTaoId, x.HocKyGoiY });

        // Khóa ngoại nội bộ module: xóa CTĐT thì xóa các dòng liên kết môn học
        builder.HasOne<ChuongTrinhDaoTao>()
            .WithMany()
            .HasForeignKey(x => x.ChuongTrinhDaoTaoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Khóa ngoại sang module Courses: Restrict
        builder.HasOne<MonHoc>()
            .WithMany()
            .HasForeignKey(x => x.MonHocId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
