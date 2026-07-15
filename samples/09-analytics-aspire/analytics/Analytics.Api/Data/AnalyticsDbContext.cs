using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace Analytics.Api.Data;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.ToTable("UsageRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProxyName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Backend).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ErrorType).HasMaxLength(100);
            entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
        });
    }
}
