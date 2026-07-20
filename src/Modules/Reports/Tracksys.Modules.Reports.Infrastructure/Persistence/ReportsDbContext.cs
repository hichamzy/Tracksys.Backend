using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Reports.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Modules.Reports.Infrastructure.Persistence;

public class ReportsDbContext(DbContextOptions<ReportsDbContext> options, ICurrentTenantAccessor tenant)
    : ModuleDbContext(options, "reporting")
{
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();
    public DbSet<ReportType> ReportTypes => Set<ReportType>();
    public DbSet<FleetMonthlyStat> FleetMonthlyStats => Set<FleetMonthlyStat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReportType>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Label).HasMaxLength(100).IsRequired();
            b.HasIndex(t => t.Label).IsUnique();
        });

        modelBuilder.Entity<SavedReport>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Name).HasMaxLength(200).IsRequired();
            b.Property(r => r.PeriodLabel).HasMaxLength(50).IsRequired();
            b.Property(r => r.Format).HasMaxLength(10).IsRequired();
            b.Property(r => r.FileUrl).HasMaxLength(500);
            b.HasOne<ReportType>()
                .WithMany()
                .HasForeignKey(r => r.ReportTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(r => r.CityId);
            b.HasQueryFilter(r => tenant.IsSuperAdmin || r.CityId == tenant.CityId);
        });

        modelBuilder.Entity<FleetMonthlyStat>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.YearMonth).HasColumnName("year_month").HasMaxLength(7).IsRequired();
            b.HasIndex(s => new { s.CityId, s.YearMonth }).IsUnique();
            b.Property(s => s.TotalDistanceKm).HasColumnType("decimal(10,2)");
            b.Property(s => s.ResolutionRatePct).HasColumnType("decimal(5,2)");
            b.HasQueryFilter(s => tenant.IsSuperAdmin || s.CityId == tenant.CityId);
        });
    }
}
