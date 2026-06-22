using Microsoft.EntityFrameworkCore;
using ShelterStack.Animals.Api.Tenancy;

namespace ShelterStack.Animals.Api.Data;

public sealed class AnimalsDbContext(DbContextOptions<AnimalsDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public DbSet<Animal> Animals => Set<Animal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Animal>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);

            // Enums stored as their string names (same pattern as Identity's UserRole) so the
            // column stays readable and survives reordering of the enum members.
            entity.Property(a => a.Species).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Sex).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Breed).HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(2000);

            // Core tenant isolation mechanism: every query against Animals is
            // implicitly scoped to the resolved tenant. Inserts are unaffected,
            // which is what lets startup seeding write rows for multiple tenants
            // through a single context instance.
            entity.HasQueryFilter(a => a.TenantId == tenantContext.TenantId);
        });
    }
}
