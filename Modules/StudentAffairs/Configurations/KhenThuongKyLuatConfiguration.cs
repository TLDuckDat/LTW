using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Configurations;

/// <summary>Cấu hình EF cho KhenThuongKyLuat.</summary>
public class KhenThuongKyLuatConfiguration : IEntityTypeConfiguration<KhenThuongKyLuat>
{
    public void Configure(EntityTypeBuilder<KhenThuongKyLuat> builder)
    {
        builder.ToTable("KhenThuongKyLuat");
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.SoQuyetDinh).HasMaxLength(50);
        builder.Property(x => x.NoiDung).HasMaxLength(1000).IsRequired();
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
    }
}
