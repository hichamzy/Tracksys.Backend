using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tracksys.Modules.Reports.Api;

public static class ReportsApiModule
{
    public static IMvcBuilder AddReportsApiModule(this IMvcBuilder builder) =>
        builder.AddApplicationPart(Assembly.GetExecutingAssembly());
}
