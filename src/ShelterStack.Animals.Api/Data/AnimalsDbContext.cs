using Microsoft.EntityFrameworkCore;
using ShelterStack.Animals.Api.Tenancy;

namespace ShelterStack.Animals.Api.Data;

public sealed class AnimalsDbContext(
    DbContextOptions<AnimalsDbContext> options,
    ITenantContext tenantContext
) : DbContext(options)
{
    public DbSet<Animal> Animals => Set<Animal>();

    public DbSet<AnimalStatusHistory> AnimalStatusHistory => Set<AnimalStatusHistory>();

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
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Breed).HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(2000);

            // Core tenant isolation mechanism: every query against Animals is
            // implicitly scoped to the resolved tenant. Inserts are unaffected,
            // which is what lets startup seeding write rows for multiple tenants
            // through a single context instance.
            entity.HasQueryFilter(a => a.TenantId == tenantContext.TenantId);
        });

        modelBuilder.Entity<AnimalStatusHistory>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);

            // No navigation property on Animal — the status endpoints look history up by
            // AnimalId directly, so a one-directional FK is all the relationship needs.
            entity.HasOne<Animal>().WithMany().HasForeignKey(h => h.AnimalId).IsRequired();

            // Same isolation mechanism as Animals: a tenant can never read another tenant's
            // status history, even for an animal id it happens to guess correctly.
            entity.HasQueryFilter(h => h.TenantId == tenantContext.TenantId);
        });
    }
}
