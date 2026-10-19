using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Auth.Entities;
using StudentManagement.Modules.StudentAffairs.Entities;

namespace StudentManagement.Modules.StudentAffairs.Configurations;

/// <summary>Cấu hình EF cho ThanhToanHocPhi.</summary>
public class ThanhToanHocPhiConfiguration : IEntityTypeConfiguration<ThanhToanHocPhi>
{
    public void Configure(EntityTypeBuilder<ThanhToanHocPhi> builder)
    {
        builder.ToTable("ThanhToanHocPhi", t => t.HasCheckConstraint("CK_ThanhToanHocPhi_SoTien", "[SoTien] > 0"));
        builder.Property(x => x.SoTien).HasPrecision(18, 2);
        builder.Property(x => x.HinhThuc).HasMaxLength(50);
        builder.Property(x => x.GhiChu).HasMaxLength(500);
        builder.HasIndex(x => x.HocPhiId);
        builder.HasOne<HocPhi>().WithMany().HasForeignKey(x => x.HocPhiId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TaiKhoan>().WithMany().HasForeignKey(x => x.NguoiThuId).OnDelete(DeleteBehavior.Restrict);
    }
}
