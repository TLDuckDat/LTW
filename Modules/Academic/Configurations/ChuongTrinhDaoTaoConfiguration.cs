using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentManagement.Modules.Academic.Entities;

namespace StudentManagement.Modules.Academic.Configurations;

/// <summary>Cấu hình EF cho ChuongTrinhDaoTao.</summary>
public class ChuongTrinhDaoTaoConfiguration : IEntityTypeConfiguration<ChuongTrinhDaoTao>
{
    public void Configure(EntityTypeBuilder<ChuongTrinhDaoTao> builder)
    {
        builder.ToTable("ChuongTrinhDaoTao");
        builder.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CpaToiThieu).HasPrecision(3, 2);
        builder.Property(x => x.DiemThucTapToiThieu).HasPrecision(4, 2);
        builder.Property(x => x.DiemBaoVeToiThieu).HasPrecision(4, 2);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ChuongTrinhDaoTao_Cpa", "[CpaToiThieu] IS NULL OR [CpaToiThieu] BETWEEN 0 AND 4");
            t.HasCheckConstraint("CK_ChuongTrinhDaoTao_DiemThucTap", "[DiemThucTapToiThieu] IS NULL OR [DiemThucTapToiThieu] BETWEEN 0 AND 10");
            t.HasCheckConstraint("CK_ChuongTrinhDaoTao_DiemBaoVe", "[DiemBaoVeToiThieu] IS NULL OR [DiemBaoVeToiThieu] BETWEEN 0 AND 10");
        });
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<Nganh>().WithMany().HasForeignKey(x => x.NganhId).OnDelete(DeleteBehavior.Restrict);
    }
}
