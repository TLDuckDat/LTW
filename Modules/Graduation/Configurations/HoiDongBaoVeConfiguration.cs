using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Graduation.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.Graduation.Configurations;

/// <summary>Cấu hình EF cho HoiDongBaoVe.</summary>
public class HoiDongBaoVeConfiguration : IEntityTypeConfiguration<HoiDongBaoVe>
{
    public void Configure(EntityTypeBuilder<HoiDongBaoVe> builder)
    {
        builder.ToTable("HoiDongBaoVe");
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.ChuTichId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<GiangVien>().WithMany().HasForeignKey(x => x.ThuKyId).OnDelete(DeleteBehavior.Restrict);
    }
}
