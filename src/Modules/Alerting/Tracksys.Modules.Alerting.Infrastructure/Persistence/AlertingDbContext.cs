using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Alerting.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Modules.Alerting.Infrastructure.Persistence;

public class AlertingDbContext(DbContextOptions<AlertingDbContext> options, ICurrentTenantAccessor tenant)
    : ModuleDbContext(options, "alerting")
{
    public DbSet<AlertType> AlertTypes => Set<AlertType>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlertType>(b =>
        {
            b.HasKey(t => t.Id);
            // AlertType.Id représente la colonne "code" (PK métier), pas une colonne "id" —
            // sans ce mapping explicite, la convention snake_case génère "id" qui n'existe pas.
            b.Property(t => t.Id).HasColumnName("code").HasMaxLength(20).ValueGeneratedNever();
            b.Property(t => t.Label).HasMaxLength(100).IsRequired();
            b.Property(t => t.Severity).HasMaxLength(2).IsRequired();
        });

        modelBuilder.Entity<AlertRule>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.AlertTypeCode).HasMaxLength(20).IsRequired();
            b.HasIndex(r => new { r.CityId, r.AlertTypeCode }).IsUnique();
            b.Property(r => r.Threshold).HasColumnType("decimal(10,2)");
            b.Property(r => r.Unit).HasMaxLength(10).IsRequired();
            b.Property(r => r.Description).HasMaxLength(300);
            b.HasOne<AlertType>()
                .WithMany()
                .HasForeignKey(r => r.AlertTypeCode)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(r => tenant.IsSuperAdmin || r.CityId == tenant.CityId);
        });

        modelBuilder.Entity<Alert>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(a => a.Code).IsUnique();
            b.Property(a => a.AlertTypeCode).HasMaxLength(20).IsRequired();
            b.Property(a => a.DetailText).HasMaxLength(500).IsRequired();
            b.HasIndex(a => a.VehicleId);
            b.HasIndex(a => a.IsUnread);
            b.HasIndex(a => a.CityId);
            b.HasOne<AlertType>()
                .WithMany()
                .HasForeignKey(a => a.AlertTypeCode)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(a => tenant.IsSuperAdmin || a.CityId == tenant.CityId);
        });
    }
}
