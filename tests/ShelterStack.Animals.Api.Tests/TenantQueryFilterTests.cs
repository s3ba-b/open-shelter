using Microsoft.EntityFrameworkCore;
using ShelterStack.Animals.Api.Data;
using ShelterStack.Animals.Api.Tenancy;
using Xunit;

namespace ShelterStack.Animals.Api.Tests;

public class TenantQueryFilterTests
{
    private static AnimalsDbContext CreateContext(string databaseName, Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<AnimalsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AnimalsDbContext(options, new StaticTenantContext(tenantId));
    }

    [Fact]
    public async Task Query_OnlyReturnsRowsForTheResolvedTenant()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // Filters apply to queries, not inserts: one context can seed rows for both tenants.
        await using (var seedContext = CreateContext(databaseName, tenantA))
        {
            seedContext.Animals.AddRange(
                new Animal
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantA,
                    Name = "Buddy",
                },
                new Animal
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantB,
                    Name = "Whiskers",
                }
            );
            await seedContext.SaveChangesAsync();
        }

        await using var tenantAContext = CreateContext(databaseName, tenantA);
        var tenantAAnimals = await tenantAContext.Animals.ToListAsync();
        Assert.Single(tenantAAnimals);
        Assert.Equal("Buddy", tenantAAnimals[0].Name);

        await using var tenantBContext = CreateContext(databaseName, tenantB);
        var tenantBAnimals = await tenantBContext.Animals.ToListAsync();
        Assert.Single(tenantBAnimals);
        Assert.Equal("Whiskers", tenantBAnimals[0].Name);
    }

    [Fact]
    public async Task IgnoreQueryFilters_BypassesTenantScoping()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using var context = CreateContext(databaseName, tenantA);
        context.Animals.AddRange(
            new Animal
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA,
                Name = "Buddy",
            },
            new Animal
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB,
                Name = "Whiskers",
            }
        );
        await context.SaveChangesAsync();

        var allAnimals = await context.Animals.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allAnimals.Count);
    }
}
