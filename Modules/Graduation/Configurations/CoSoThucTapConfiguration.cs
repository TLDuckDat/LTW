using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Graduation.Entities;

namespace StudentManagement.Modules.Graduation.Configurations;

public class CoSoThucTapConfiguration : IEntityTypeConfiguration<CoSoThucTap>
{
    public void Configure(EntityTypeBuilder<CoSoThucTap> builder)
    {
        builder.ToTable("CoSoThucTap");
        builder.Property(x => x.Ma).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DiaChi).HasMaxLength(300);
        builder.Property(x => x.SoDienThoai).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.NguoiLienHe).HasMaxLength(100);
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_CoSoThucTap_Ma", "LEN(TRIM([Ma])) > 0");
            t.HasCheckConstraint("CK_CoSoThucTap_Ten", "LEN(TRIM([Ten])) > 0");
        });
    }
}
