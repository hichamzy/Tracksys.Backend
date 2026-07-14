using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Ingestion.Application.Mapping;
using Tracksys.Modules.Ingestion.Application.Services;
using Tracksys.Modules.Ingestion.Infrastructure.Lookup;
using Tracksys.Modules.Ingestion.Infrastructure.Persistence;

namespace Tracksys.Modules.Ingestion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIngestionModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' introuvable.");

        // NpgsqlDataSource dédié au chemin d'ingestion : UseNetTopologySuite() active le
        // mapping GEOGRAPHY(POINT) <-> NetTopologySuite.Geometries.Point utilisé par
        // NpgsqlTelemetryWriter (COPY binaire) et LookupService (SQL brut).
        NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(connectionString)
            .UseNetTopologySuite()
            .Build();
        services.AddSingleton(dataSource);

        services.AddDbContext<IngestionDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "ingestion"))
                .UseSnakeCaseNamingConvention());

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ITelemetryWriter, NpgsqlTelemetryWriter>();
        services.AddScoped<IPositionQueryService, NpgsqlPositionQueryService>();
        services.AddScoped<FlespiMapper>();
        services.AddScoped<IngestFlespiBatchHandler>();

        return services;
    }
}
