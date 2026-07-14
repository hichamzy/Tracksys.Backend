using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Reports.Application.Abstractions;
using Tracksys.Modules.Reports.Application.Services;
using Tracksys.Modules.Reports.Infrastructure.Persistence;

namespace Tracksys.Modules.Reports.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' introuvable.");

        services.AddDbContext<ReportsDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "reporting"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IReportsUnitOfWork, ReportsUnitOfWork>();
        services.AddScoped<ReportQueryService>();
        services.AddScoped<ReportCommandService>();

        return services;
    }
}
