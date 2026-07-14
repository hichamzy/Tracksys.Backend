using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Identity.Domain.Entities;

namespace Tracksys.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// DbContext Identity — mappe les tables AspNet* + RefreshTokens sur le schéma "identity"
/// (voir database/postgresql/001_tracksys_schema.sql).
/// </summary>
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        // IdentityDbContext<TUser,TRole,TKey>.OnModelCreating (classe de base ASP.NET Core
        // Identity) pose ToTable("AspNetUsers") etc. explicitement — un nom de table fixé par
        // du code n'est PAS renormalisé par UseSnakeCaseNamingConvention() (qui ne réécrit que
        // les noms par défaut). Il faut donc forcer explicitement le nom snake_case ici pour
        // matcher les tables réelles (identity.asp_net_users, etc.).
        builder.Entity<ApplicationUser>(b => b.ToTable("asp_net_users"));
        builder.Entity<ApplicationRole>(b => b.ToTable("asp_net_roles"));
        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("asp_net_user_roles"));
        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("asp_net_user_claims"));
        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("asp_net_role_claims"));
        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("asp_net_user_logins"));
        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("asp_net_user_tokens"));

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            b.Property(u => u.Scope).HasMaxLength(200);
            b.Property(u => u.IsActive).HasDefaultValue(true);
            b.Property(u => u.CreatedAtUtc).HasDefaultValueSql("timezone('utc', now())");
        });

        builder.Entity<RefreshToken>(b =>
        {
            b.HasKey(rt => rt.Id);
            b.Property(rt => rt.TokenHash).HasMaxLength(256).IsRequired();
            b.HasIndex(rt => rt.TokenHash).IsUnique();
            b.HasIndex(rt => rt.UserId);
            b.Property(rt => rt.CreatedByIp).HasMaxLength(64);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
