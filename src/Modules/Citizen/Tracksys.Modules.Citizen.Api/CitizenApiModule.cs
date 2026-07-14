using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tracksys.Modules.Citizen.Application.Services;

namespace Tracksys.Modules.Citizen.Api;

public static class CitizenApiModule
{
    public static IMvcBuilder AddCitizenApiModule(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<ComplaintQueryService>();
        builder.Services.AddScoped<ComplaintCommandService>();
        builder.Services.AddScoped<ComplaintCategoryQueryService>();
        builder.Services.AddScoped<ComplaintCategoryCommandService>();
        return builder.AddApplicationPart(Assembly.GetExecutingAssembly());
    }
}
