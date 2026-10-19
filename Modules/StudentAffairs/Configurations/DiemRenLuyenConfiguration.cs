using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;
using StudentManagement.Modules.StudentAffairs.Entities;
using StudentManagement.Modules.Students.Entities;

namespace StudentManagement.Modules.StudentAffairs.Configurations;

/// <summary>Cấu hình EF cho DiemRenLuyen.</summary>
public class DiemRenLuyenConfiguration : IEntityTypeConfiguration<DiemRenLuyen>
{
    public void Configure(EntityTypeBuilder<DiemRenLuyen> builder)
    {
        builder.ToTable("DiemRenLuyen", t => t.HasCheckConstraint("CK_DiemRenLuyen_Diem", "[Diem] BETWEEN 0 AND 100"));
        builder.Property(x => x.XepLoai).HasMaxLength(30);
        builder.HasIndex(x => new { x.SinhVienId, x.HocKyId }).IsUnique();
        builder.HasOne<SinhVien>().WithMany().HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<HocKy>().WithMany().HasForeignKey(x => x.HocKyId).OnDelete(DeleteBehavior.Restrict);
    }
}
