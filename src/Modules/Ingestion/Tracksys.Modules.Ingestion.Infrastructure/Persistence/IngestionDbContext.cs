using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Ingestion.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;

namespace Tracksys.Modules.Ingestion.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core pour ingestion.ingest_anomaly UNIQUEMENT. Les tables
/// telemetry/last_position (écriture haute fréquence, COPY binaire, transaction
/// multi-étapes) ne passent jamais par EF Core — voir NpgsqlTelemetryWriter.
/// </summary>
public class IngestionDbContext(DbContextOptions<IngestionDbContext> options) : ModuleDbContext(options, "ingestion")
{
    public DbSet<IngestAnomaly> IngestAnomalies => Set<IngestAnomaly>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IngestAnomaly>(b =>
        {
            b.ToTable("ingest_anomaly");
            b.HasKey(a => a.Id);
            b.Property(a => a.TimeUtc).HasColumnName("time").IsRequired();
            b.Property(a => a.Ident).HasColumnName("ident");
            b.Property(a => a.Raison).HasColumnName("raison").IsRequired();
            b.Property(a => a.PayloadBrut).HasColumnName("payload_brut").HasColumnType("jsonb").IsRequired();
        });
    }
}
