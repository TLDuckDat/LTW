using Microsoft.EntityFrameworkCore;
using StudentManagement.Common;

namespace StudentManagement.Data;

/// <summary>DbContext chung cho toàn bộ hệ thống. Mỗi module tự khai báo cấu hình trong thư mục Configurations.</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Không khai báo DbSet: dùng Set<T>() trong Service, ví dụ _db.Set<Khoa>().
    // Mọi thực thể được đăng ký qua IEntityTypeConfiguration<T> trong các module.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
