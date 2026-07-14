using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Citizen.Application.Abstractions;
using Tracksys.Modules.Citizen.Infrastructure.Persistence;

namespace Tracksys.Modules.Citizen.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCitizenModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' introuvable.");

        services.AddDbContext<CitizenDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "citizen"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICitizenUnitOfWork, CitizenUnitOfWork>();

        return services;
    }
}
