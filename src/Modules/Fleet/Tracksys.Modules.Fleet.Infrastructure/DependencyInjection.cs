using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Infrastructure.Persistence;

namespace Tracksys.Modules.Fleet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFleetModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' introuvable.");

        services.AddDbContext<FleetDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "fleet"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IFleetUnitOfWork, FleetUnitOfWork>();

        return services;
    }
}
