using Microsoft.AspNetCore.Identity;

namespace Tracksys.Modules.Identity.Domain.Entities;

/// <summary>Utilisateur applicatif — étend IdentityUser avec les champs métier du front (FullName, Scope).</summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Périmètre affiché (ex. "Anfa · Maârif"). Champ libre pour l'instant, pas de relation vers une table Zones.</summary>
    public string? Scope { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Ville d'appartenance. Null uniquement pour un compte SuperAdmin (accès toutes villes).</summary>
    public Guid? CityId { get; set; }
}
