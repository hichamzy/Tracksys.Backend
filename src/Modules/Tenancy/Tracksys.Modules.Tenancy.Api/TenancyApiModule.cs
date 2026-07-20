using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Tenancy.Application.Services;

namespace Tracksys.Modules.Tenancy.Api;

public static class TenancyApiModule
{
    public static IMvcBuilder AddTenancyApiModule(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<CityQueryService>();
        builder.Services.AddScoped<CityModuleService>();
        builder.Services.AddScoped<CityCommandService>();
        return builder.AddApplicationPart(Assembly.GetExecutingAssembly());
    }
}
