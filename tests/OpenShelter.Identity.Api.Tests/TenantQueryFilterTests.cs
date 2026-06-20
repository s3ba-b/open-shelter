using Microsoft.EntityFrameworkCore;
using OpenShelter.Identity.Api.Data;
using OpenShelter.Identity.Api.Tenancy;
using Xunit;

namespace OpenShelter.Identity.Api.Tests;

public class TenantQueryFilterTests
{
    private static IdentityDbContext CreateContext(string databaseName, Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new IdentityDbContext(options, new StaticTenantContext(tenantId));
    }

    [Fact]
    public async Task Query_OnlyReturnsUsersForTheResolvedTenant()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // Filters apply to queries, not inserts: one context can seed rows for both tenants.
        await using (var seedContext = CreateContext(databaseName, tenantA))
        {
            seedContext.Users.AddRange(
                new User { Id = Guid.NewGuid(), TenantId = tenantA, Email = "a@example.com", PasswordHash = "x", Role = UserRole.Admin },
                new User { Id = Guid.NewGuid(), TenantId = tenantB, Email = "b@example.com", PasswordHash = "x", Role = UserRole.Staff });
            await seedContext.SaveChangesAsync();
        }

        await using var tenantAContext = CreateContext(databaseName, tenantA);
        var tenantAUsers = await tenantAContext.Users.ToListAsync();
        Assert.Single(tenantAUsers);
        Assert.Equal("a@example.com", tenantAUsers[0].Email);

        await using var tenantBContext = CreateContext(databaseName, tenantB);
        var tenantBUsers = await tenantBContext.Users.ToListAsync();
        Assert.Single(tenantBUsers);
        Assert.Equal("b@example.com", tenantBUsers[0].Email);
    }

    [Fact]
    public async Task IgnoreQueryFilters_BypassesTenantScoping()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using var context = CreateContext(databaseName, tenantA);
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), TenantId = tenantA, Email = "a@example.com", PasswordHash = "x", Role = UserRole.Admin },
            new User { Id = Guid.NewGuid(), TenantId = tenantB, Email = "b@example.com", PasswordHash = "x", Role = UserRole.Staff });
        await context.SaveChangesAsync();

        var allUsers = await context.Users.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, allUsers.Count);
    }
}
