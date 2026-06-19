using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OpenShelter.Animals.Api.Tenancy;

namespace OpenShelter.Animals.Api.Data;

/// <summary>
/// Lets `dotnet ef migrations add` construct the context without Aspire/DI or a
/// live database — only used by EF Core tooling, never at runtime.
/// </summary>
public sealed class AnimalsDbContextFactory : IDesignTimeDbContextFactory<AnimalsDbContext>
{
    public AnimalsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnimalsDbContext>()
            .UseNpgsql("Host=localhost;Database=openshelterdb;Username=design-time");

        return new AnimalsDbContext(optionsBuilder.Options, new StaticTenantContext(Guid.Empty));
    }
}
