using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Tenancy.Domain.Entities;
using Tracksys.Shared.Infrastructure.Persistence;

namespace Tracksys.Modules.Tenancy.Infrastructure.Persistence;

public class TenancyDbContext(DbContextOptions<TenancyDbContext> options) : ModuleDbContext(options, "tenancy")
{
    public DbSet<City> Cities => Set<City>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).HasMaxLength(150).IsRequired();
            b.Property(c => c.Code).HasMaxLength(20).IsRequired();
            b.HasIndex(c => c.Code).IsUnique();
        });
    }
}
