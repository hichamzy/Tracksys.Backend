using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tracksys.Modules.Identity.Api;

public static class IdentityApiModule
{
    /// <summary>Enregistre les contrôleurs du module Identity dans le pipeline MVC du Host.</summary>
    public static IMvcBuilder AddIdentityApiModule(this IMvcBuilder builder) =>
        builder.AddApplicationPart(Assembly.GetExecutingAssembly());
}
