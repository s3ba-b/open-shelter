using Microsoft.EntityFrameworkCore;
using ShelterStack.Identity.Api.Tenancy;

namespace ShelterStack.Identity.Api.Data;

public sealed class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    ITenantContext tenantContext
) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(320);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

            // Core tenant isolation mechanism, same pattern as AnimalsDbContext: every query
            // against Users is implicitly scoped to the resolved tenant. Inserts are
            // unaffected, which is what lets startup seeding write rows for multiple tenants
            // through a single context instance.
            entity.HasQueryFilter(u => u.TenantId == tenantContext.TenantId);
        });
    }
}
