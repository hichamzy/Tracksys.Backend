using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Tenancy.Application.Abstractions;
using Tracksys.Modules.Tenancy.Infrastructure.Auth;
using Tracksys.Modules.Tenancy.Infrastructure.Persistence;
using Tracksys.Shared.Kernel.Auth;

namespace Tracksys.Modules.Tenancy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' introuvable.");

        services.AddDbContext<TenancyDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "tenancy"))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ITenancyUnitOfWork, TenancyUnitOfWork>();
        services.AddScoped<ICityModuleResolver, CityModuleResolver>();

        return services;
    }
}
