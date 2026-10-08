using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Courses.Entities;

namespace StudentManagement.Modules.Courses.Configurations;

/// <summary>Cấu hình EF cho LichHoc.</summary>
public class LichHocConfiguration : IEntityTypeConfiguration<LichHoc>
{
    public void Configure(EntityTypeBuilder<LichHoc> builder)
    {
        builder.ToTable("LichHoc");
        builder.HasOne<LopHocPhan>().WithMany().HasForeignKey(x => x.LopHocPhanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PhongHoc>().WithMany().HasForeignKey(x => x.PhongHocId).OnDelete(DeleteBehavior.Restrict);
    }
}
