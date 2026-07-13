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
        string connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' introuvable.");

        services.AddDbContext<FleetDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "fleet")));

        services.AddScoped<IFleetUnitOfWork, FleetUnitOfWork>();

        return services;
    }
}
