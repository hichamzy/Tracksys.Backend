using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Fleet.Domain.Entities;
using Tracksys.Modules.Fleet.Domain.Enums;
using Tracksys.Shared.Infrastructure.Persistence;

namespace Tracksys.Modules.Fleet.Infrastructure.Persistence;

public class FleetDbContext(DbContextOptions<FleetDbContext> options) : ModuleDbContext(options, "fleet")
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<Driver> Drivers => Set<Driver>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VehicleType>(b =>
        {
            b.ToTable("VehicleTypes");
            b.HasKey(t => t.Id);
            b.Property(t => t.Label).HasMaxLength(100).IsRequired();
            b.HasIndex(t => t.Label).IsUnique();
        });

        modelBuilder.Entity<Driver>(b =>
        {
            b.ToTable("Drivers");
            b.HasKey(d => d.Id);
            b.Property(d => d.FullName).HasMaxLength(150).IsRequired();
            b.Property(d => d.Phone).HasMaxLength(32);
            b.Property(d => d.LicenceNumber).HasMaxLength(50);
            b.Property(d => d.Status).HasMaxLength(30).HasDefaultValue("En service");
            b.HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(d => d.CurrentVehicleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Vehicle>(b =>
        {
            b.ToTable("Vehicles");
            b.HasKey(v => v.Id);
            b.Property(v => v.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(v => v.Code).IsUnique();
            b.Property(v => v.PlateNumber).HasMaxLength(20).IsRequired();
            b.HasIndex(v => v.PlateNumber).IsUnique();
            b.Property(v => v.Zone).HasMaxLength(100);
            b.Property(v => v.ImeiTracker).HasMaxLength(50);
            b.Property(v => v.SpeedKmh).HasColumnType("decimal(6,2)");
            b.Property(v => v.DistanceTodayKm).HasColumnType("decimal(8,2)");
            b.Property(v => v.LastKnownLat).HasColumnType("decimal(9,6)");
            b.Property(v => v.LastKnownLng).HasColumnType("decimal(9,6)");

            b.Property(v => v.Status)
                .HasConversion(s => s.ToCode(), code => VehicleStatusExtensions.FromCode(code))
                .HasColumnName("StatusCode")
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
        });
    }
}
