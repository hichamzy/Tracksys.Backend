using Microsoft.EntityFrameworkCore;

namespace Tracksys.Shared.Infrastructure.Persistence;

/// <summary>
/// Base commune des DbContext de module : chaque module mappe ses entités sur son
/// propre schéma SQL Server (fleet, citizen, alerting...) au sein de la même base TracksysDb.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options, string schema) : DbContext(options)
{
    protected readonly string Schema = schema;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);
    }
}
