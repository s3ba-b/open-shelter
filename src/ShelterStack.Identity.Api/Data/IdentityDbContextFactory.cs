using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShelterStack.Identity.Api.Tenancy;

namespace ShelterStack.Identity.Api.Data;

/// <summary>
/// Lets `dotnet ef migrations add` construct the context without Aspire/DI or a
/// live database — only used by EF Core tooling, never at runtime.
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=localhost;Database=identitydb;Username=design-time");

        return new IdentityDbContext(optionsBuilder.Options, new StaticTenantContext(Guid.Empty));
    }
}
