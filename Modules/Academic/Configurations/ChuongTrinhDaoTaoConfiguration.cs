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
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne<Nganh>().WithMany().HasForeignKey(x => x.NganhId).OnDelete(DeleteBehavior.Restrict);
    }
}
