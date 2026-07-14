using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Alerting.Application.Abstractions;
using Tracksys.Modules.Alerting.Infrastructure.Persistence;

namespace Tracksys.Modules.Alerting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAlertingModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' introuvable.");

        services.AddDbContext<AlertingDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "alerting"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IAlertingUnitOfWork, AlertingUnitOfWork>();

        return services;
    }
}
