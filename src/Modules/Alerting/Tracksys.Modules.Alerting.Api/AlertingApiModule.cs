using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Alerting.Application.Services;

namespace Tracksys.Modules.Alerting.Api;

public static class AlertingApiModule
{
    public static IMvcBuilder AddAlertingApiModule(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<AlertQueryService>();
        builder.Services.AddScoped<AlertCommandService>();
        builder.Services.AddScoped<AlertTypeQueryService>();
        builder.Services.AddScoped<AlertRuleQueryService>();
        builder.Services.AddScoped<AlertRuleCommandService>();
        return builder.AddApplicationPart(Assembly.GetExecutingAssembly());
    }
}
