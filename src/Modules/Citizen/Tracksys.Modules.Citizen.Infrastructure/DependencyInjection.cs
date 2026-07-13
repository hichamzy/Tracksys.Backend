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
        string connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Connection string 'SqlServer' introuvable.");

        services.AddDbContext<CitizenDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "citizen")));

        services.AddScoped<ICitizenUnitOfWork, CitizenUnitOfWork>();

        return services;
    }
}
