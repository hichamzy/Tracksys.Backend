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
        string connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' introuvable.");

        services.AddDbContext<AlertingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "alerting")));

        services.AddScoped<IAlertingUnitOfWork, AlertingUnitOfWork>();

        return services;
    }
}
