using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tracksys.Modules.Identity.Domain.Entities;

namespace Tracksys.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// DbContext Identity — mappe les tables AspNet* + RefreshTokens sur le schéma [identity]
/// (voir database/sqlserver/001_tracksys_schema.sql).
/// </summary>
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("AspNetUsers");
            b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            b.Property(u => u.Scope).HasMaxLength(200);
            b.Property(u => u.IsActive).HasDefaultValue(true);
            b.Property(u => u.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        builder.Entity<ApplicationRole>(b => b.ToTable("AspNetRoles"));
        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("AspNetUserRoles"));
        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("AspNetUserClaims"));
        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("AspNetRoleClaims"));
        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("AspNetUserLogins"));
        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("AspNetUserTokens"));

        builder.Entity<RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");
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
