using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Ingestion.Application.Options;

namespace Tracksys.Modules.Ingestion.Api;

public static class IngestionApiModule
{
    public static IMvcBuilder AddIngestionApiModule(this IMvcBuilder builder, IConfiguration configuration)
    {
        builder.Services.Configure<IngestionOptions>(configuration.GetSection(IngestionOptions.SectionName));
        return builder.AddApplicationPart(Assembly.GetExecutingAssembly());
    }
}
