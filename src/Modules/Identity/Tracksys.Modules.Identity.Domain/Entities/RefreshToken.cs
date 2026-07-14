using Tracksys.Shared.Kernel.Entities;
using Tracksys.Shared.Kernel.Guards;

namespace Tracksys.Modules.Identity.Domain.Entities;

public class RefreshToken : Entity<long>, IAggregateRoot
{
    public string UserId { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    private RefreshToken() { } // EF Core

    public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAtUtc, string? createdByIp) =>
        new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTimeGuard.EnsureUtc(expiresAtUtc, nameof(expiresAtUtc)),
            CreatedByIp = createdByIp,
        };

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
