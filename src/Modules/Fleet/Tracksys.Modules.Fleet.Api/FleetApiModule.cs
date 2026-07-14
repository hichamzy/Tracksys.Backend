using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Fleet.Application.Services;

namespace Tracksys.Modules.Fleet.Api;

public static class FleetApiModule
{
    public static IMvcBuilder AddFleetApiModule(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<VehicleQueryService>();
        builder.Services.AddScoped<VehicleCommandService>();
        builder.Services.AddScoped<DriverQueryService>();
        builder.Services.AddScoped<DriverCommandService>();
        builder.Services.AddScoped<VehicleTypeQueryService>();
        return builder.AddApplicationPart(Assembly.GetExecutingAssembly());
    }
}
