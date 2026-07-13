using Microsoft.AspNetCore.Identity;

namespace Tracksys.Modules.Identity.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
