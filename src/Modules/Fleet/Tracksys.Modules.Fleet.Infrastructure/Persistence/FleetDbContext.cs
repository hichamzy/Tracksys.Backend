using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Fleet.Domain.Entities;
using Tracksys.Modules.Fleet.Domain.Enums;
using Tracksys.Shared.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Modules.Fleet.Infrastructure.Persistence;

public class FleetDbContext(DbContextOptions<FleetDbContext> options, ICurrentTenantAccessor tenant)
    : ModuleDbContext(options, "fleet")
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Chariot> Chariots => Set<Chariot>();
    public DbSet<Delegataire> Delegataires => Set<Delegataire>();
    public DbSet<Circuit> Circuits => Set<Circuit>();
    public DbSet<TypePrestation> TypesPrestation => Set<TypePrestation>();
    public DbSet<Planning> Plannings => Set<Planning>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Noms de table/colonne dérivés automatiquement par UseSnakeCaseNamingConvention()
        // (ex. VehicleTypes -> vehicle_types, StatusCode -> status_code) : pas de ToTable/HasColumnName explicites.

        modelBuilder.Entity<VehicleType>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Label).HasMaxLength(100).IsRequired();
            b.HasIndex(t => t.Label).IsUnique();
        });

        modelBuilder.Entity<Driver>(b =>
        {
            b.HasKey(d => d.Id);
            b.Property(d => d.FullName).HasMaxLength(150).IsRequired();
            b.Property(d => d.Phone).HasMaxLength(32);
            b.Property(d => d.LicenceNumber).HasMaxLength(50);
            b.Property(d => d.Status).HasMaxLength(30).HasDefaultValue("En service");
            b.HasIndex(d => d.CityId);
            b.HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(d => d.CurrentVehicleId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasQueryFilter(d => tenant.IsSuperAdmin || d.CityId == tenant.CityId);
        });

        modelBuilder.Entity<Vehicle>(b =>
        {
            b.HasKey(v => v.Id);
            b.Property(v => v.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(v => v.Code).IsUnique();
            b.Property(v => v.PlateNumber).HasMaxLength(20).IsRequired();
            b.HasIndex(v => v.PlateNumber).IsUnique();
            b.Property(v => v.Zone).HasMaxLength(100);
            b.Property(v => v.ImeiTracker).HasMaxLength(50);
            b.Property(v => v.FlespiIdent).HasMaxLength(50);
            b.HasIndex(v => v.FlespiIdent).IsUnique().HasFilter("flespi_ident IS NOT NULL");
            b.HasIndex(v => v.CityId);
            b.Property(v => v.SpeedKmh).HasColumnType("decimal(6,2)");
            b.Property(v => v.DistanceTodayKm).HasColumnType("decimal(8,2)");
            b.Property(v => v.LastKnownLat).HasColumnType("decimal(9,6)");
            b.Property(v => v.LastKnownLng).HasColumnType("decimal(9,6)");

            b.Property(v => v.Status)
                .HasConversion(s => s.ToCode(), code => VehicleStatusExtensions.FromCode(code))
                .HasColumnName("status_code")
                .HasMaxLength(10)
                .IsRequired();

            b.HasOne<VehicleType>()
                .WithMany()
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<Driver>()
                .WithMany()
                .HasForeignKey(v => v.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(v => tenant.IsSuperAdmin || v.CityId == tenant.CityId);
        });

        modelBuilder.Entity<Delegataire>(b =>
        {
            b.HasKey(d => d.Id);
            b.Property(d => d.Label).HasMaxLength(150).IsRequired();
            b.HasIndex(d => d.Label).IsUnique();
            b.HasIndex(d => d.CityId);
            b.HasQueryFilter(d => tenant.IsSuperAdmin || d.CityId == tenant.CityId);
        });

        modelBuilder.Entity<Circuit>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Label).HasMaxLength(150).IsRequired();
            b.HasIndex(c => c.Label).IsUnique();
            b.HasIndex(c => c.CityId);
            b.HasQueryFilter(c => tenant.IsSuperAdmin || c.CityId == tenant.CityId);
        });

        modelBuilder.Entity<TypePrestation>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Label).HasMaxLength(150).IsRequired();
            b.HasIndex(t => t.Label).IsUnique();
            b.HasIndex(t => t.CityId);
            b.HasQueryFilter(t => tenant.IsSuperAdmin || t.CityId == tenant.CityId);
        });

        modelBuilder.Entity<Chariot>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Numero).HasMaxLength(50).IsRequired();
            b.HasIndex(c => c.Numero).IsUnique();
            b.Property(c => c.FlespiIdent).HasMaxLength(50);
            b.HasIndex(c => c.FlespiIdent).IsUnique().HasFilter("flespi_ident IS NOT NULL");
            b.Property(c => c.LastKnownLat).HasColumnType("decimal(9,6)");
            b.Property(c => c.LastKnownLng).HasColumnType("decimal(9,6)");

            b.HasOne<Delegataire>()
                .WithMany()
                .HasForeignKey(c => c.DelegataireId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(c => c.CityId);
            b.HasQueryFilter(c => tenant.IsSuperAdmin || c.CityId == tenant.CityId);
        });

        modelBuilder.Entity<Planning>(b =>
        {
            b.HasKey(p => p.Id);
            b.HasIndex(p => new { p.ChariotId, p.DebutUtc, p.FinUtc });
            b.HasIndex(p => p.CityId);

            b.HasOne<Chariot>()
                .WithMany()
                .HasForeignKey(p => p.ChariotId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Circuit>()
                .WithMany()
                .HasForeignKey(p => p.CircuitId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne<TypePrestation>()
                .WithMany()
                .HasForeignKey(p => p.TypePrestationId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(p => tenant.IsSuperAdmin || p.CityId == tenant.CityId);
        });
    }
}
