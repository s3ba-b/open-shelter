using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenShelter.Animals.Api.Data;
using OpenShelter.Animals.Api.Tenancy;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Connect to the PostgreSQL "openshelterdb" resource via Aspire service discovery,
// registering an NpgsqlDataSource plus a health check that proves the connection.
builder.AddNpgsqlDataSource("openshelterdb");

// Unpooled by design: AnimalsDbContext takes the per-request ITenantContext as a
// constructor dependency, and DbContext pooling reuses instances (and whatever
// scoped service they captured) across unrelated requests' scopes — exactly the
// kind of cross-tenant leak this milestone exists to prevent.
builder.Services.AddDbContext<AnimalsDbContext>((sp, options) =>
    options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddHttpContextAccessor();

// M0 placeholder tenant resolution (header-based). M1 swaps this for resolution
// from an authenticated tenant claim without touching ITenantContext consumers.
builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();

var app = builder.Build();

app.MapDefaultEndpoints();

await SeedDemoTenantsAsync(app.Services);

// Trivial liveness/ping endpoint, reachable through the gateway at /animals/ping.
app.MapGet("/ping", () => Results.Ok(new { service = "animals-api", status = "ok" }));

// Lists animals for the tenant resolved from the X-Tenant-Id header, demonstrating
// that the EF Core global query filter scopes results without any explicit
// per-call filtering. Reachable through the gateway at /animals.
app.MapGet("/", async (AnimalsDbContext db) =>
    Results.Ok(await db.Animals.Select(a => new { a.Id, a.Name }).ToListAsync()));

app.Run();

static async Task SeedDemoTenantsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<AnimalsDbContext>>();

    // Filters apply to queries, not inserts, so a single context (with any tenant)
    // can migrate the schema and seed rows across multiple demo tenants.
    await using var db = new AnimalsDbContext(options, new StaticTenantContext(Guid.Empty));
    await db.Database.MigrateAsync();

    if (await db.Animals.IgnoreQueryFilters().AnyAsync())
    {
        return;
    }

    db.Animals.AddRange(
        new Animal { Id = Guid.NewGuid(), TenantId = DemoTenants.Northside, Name = "Buddy" },
        new Animal { Id = Guid.NewGuid(), TenantId = DemoTenants.Riverside, Name = "Whiskers" });

    await db.SaveChangesAsync();
}

// Makes the top-level-statement Program class public so the isolation tests' (and any
// future integration tests') WebApplicationFactory<Program> can boot the real DI-wired
// host instead of a hand-rolled stand-in.
public partial class Program;
