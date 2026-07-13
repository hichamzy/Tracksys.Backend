using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Alerting.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;

namespace Tracksys.Modules.Alerting.Infrastructure.Persistence;

public class AlertingDbContext(DbContextOptions<AlertingDbContext> options) : ModuleDbContext(options, "alerting")
{
    public DbSet<AlertType> AlertTypes => Set<AlertType>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlertType>(b =>
        {
            b.ToTable("AlertTypes");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).HasMaxLength(20).ValueGeneratedNever();
            b.Property(t => t.Label).HasMaxLength(100).IsRequired();
            b.Property(t => t.Severity).HasMaxLength(2).IsRequired();
        });

        modelBuilder.Entity<AlertRule>(b =>
        {
            b.ToTable("AlertRules");
            b.HasKey(r => r.Id);
            b.Property(r => r.AlertTypeCode).HasMaxLength(20).IsRequired();
            b.HasIndex(r => r.AlertTypeCode).IsUnique();
            b.Property(r => r.Threshold).HasColumnType("decimal(10,2)");
            b.Property(r => r.Unit).HasMaxLength(10).IsRequired();
            b.Property(r => r.Description).HasMaxLength(300);
            b.HasOne<AlertType>()
                .WithMany()
                .HasForeignKey(r => r.AlertTypeCode)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Alert>(b =>
        {
            b.ToTable("Alerts");
            b.HasKey(a => a.Id);
            b.Property(a => a.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(a => a.Code).IsUnique();
            b.Property(a => a.AlertTypeCode).HasMaxLength(20).IsRequired();
            b.Property(a => a.DetailText).HasMaxLength(500).IsRequired();
            b.HasIndex(a => a.VehicleId);
            b.HasIndex(a => a.IsUnread);
            b.HasOne<AlertType>()
                .WithMany()
                .HasForeignKey(a => a.AlertTypeCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
