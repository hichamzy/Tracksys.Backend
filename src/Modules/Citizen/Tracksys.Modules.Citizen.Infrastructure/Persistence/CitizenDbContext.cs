using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Citizen.Domain.Entities;
using Tracksys.Modules.Citizen.Domain.Enums;
using Tracksys.Shared.Infrastructure.Persistence;

namespace Tracksys.Modules.Citizen.Infrastructure.Persistence;

public class CitizenDbContext(DbContextOptions<CitizenDbContext> options) : ModuleDbContext(options, "citizen")
{
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintCategory> ComplaintCategories => Set<ComplaintCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ComplaintCategory>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Label).HasMaxLength(100).IsRequired();
            b.HasIndex(c => c.Label).IsUnique();
            b.Property(c => c.DefaultPriority).HasMaxLength(10).IsRequired();
        });

        modelBuilder.Entity<Complaint>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(c => c.Code).IsUnique();
            b.Property(c => c.Priority).HasMaxLength(10).IsRequired();
            b.Property(c => c.ZoneLabel).HasMaxLength(150).IsRequired();
            b.Property(c => c.Lat).HasColumnType("decimal(9,6)");
            b.Property(c => c.Lng).HasColumnType("decimal(9,6)");
            b.Property(c => c.ReporterName).HasMaxLength(100);
            b.Property(c => c.PhotoBeforeUrl).HasMaxLength(500);
            b.Property(c => c.PhotoAfterUrl).HasMaxLength(500);

            b.Property(c => c.Status)
                .HasConversion(s => s.ToCode(), code => ComplaintStatusExtensions.FromCode(code))
                .HasColumnName("status_code")
                .HasMaxLength(10)
                .IsRequired();

            b.HasOne<ComplaintCategory>()
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
