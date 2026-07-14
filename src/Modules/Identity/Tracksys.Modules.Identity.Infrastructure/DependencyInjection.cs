using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tracksys.Modules.Identity.Application.Abstractions;
using Tracksys.Modules.Identity.Application.Options;
using Tracksys.Modules.Identity.Application.Services;
using Tracksys.Modules.Identity.Domain.Entities;
using Tracksys.Modules.Identity.Infrastructure.Auth;
using Tracksys.Modules.Identity.Infrastructure.Persistence;

namespace Tracksys.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' introuvable.");

        services.AddDbContext<IdentityDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "identity"))
                .UseSnakeCaseNamingConvention());

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();

        return services;
    }
}
