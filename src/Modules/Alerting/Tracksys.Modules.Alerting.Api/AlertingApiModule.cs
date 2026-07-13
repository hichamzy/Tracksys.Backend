using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Alerting.Application.Services;

namespace Tracksys.Modules.Alerting.Api;

public static class AlertingApiModule
{
    public static IMvcBuilder AddAlertingApiModule(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<AlertQueryService>();
        return builder.AddApplicationPart(Assembly.GetExecutingAssembly());
    }
}
