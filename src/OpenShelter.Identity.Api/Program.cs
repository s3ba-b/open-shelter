using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenShelter.Identity.Api.Data;
using OpenShelter.Identity.Api.Tenancy;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Connect to the PostgreSQL "identitydb" resource via Aspire service discovery,
// registering an NpgsqlDataSource plus a health check that proves the connection.
builder.AddNpgsqlDataSource("identitydb");

// Unpooled by design: IdentityDbContext takes the per-request ITenantContext as a
// constructor dependency, and DbContext pooling reuses instances (and whatever
// scoped service they captured) across unrelated requests' scopes — exactly the
// kind of cross-tenant leak this milestone exists to prevent.
builder.Services.AddDbContext<IdentityDbContext>((sp, options) =>
    options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddHttpContextAccessor();

// M0/M1 placeholder tenant resolution (header-based), same as OpenShelter.Animals.Api.
// The auth pipeline (next issue) swaps this for resolution from an authenticated tenant
// claim without touching ITenantContext consumers.
builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();

var app = builder.Build();

app.MapDefaultEndpoints();

await SeedDemoDataAsync(app.Services);

// Trivial liveness/ping endpoint, reachable through the gateway at /identity/ping.
app.MapGet("/ping", () => Results.Ok(new { service = "identity-api", status = "ok" }));

// Lists users for the tenant resolved from the X-Tenant-Id header, demonstrating that the
// EF Core global query filter scopes results without any explicit per-call filtering.
// Reachable through the gateway at /identity/users.
app.MapGet("/users", async (IdentityDbContext db) =>
    Results.Ok(await db.Users
        .Select(u => new { u.Id, u.Email, u.Role })
        .ToListAsync()));

app.Run();

static async Task SeedDemoDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<IdentityDbContext>>();

    // Filters apply to queries, not inserts, so a single context (with any tenant)
    // can migrate the schema and seed rows across multiple demo tenants.
    await using var db = new IdentityDbContext(options, new StaticTenantContext(Guid.Empty));
    await db.Database.MigrateAsync();

    if (await db.Organizations.AnyAsync())
    {
        return;
    }

    db.Organizations.AddRange(
        new Organization { Id = DemoTenants.Northside, Name = "Northside Shelter" },
        new Organization { Id = DemoTenants.Riverside, Name = "Riverside Rescue" });

    var hasher = new PasswordHasher<User>();
    db.Users.AddRange(
        SeedUser(hasher, DemoTenants.Northside, "admin@northside.example", UserRole.Admin),
        SeedUser(hasher, DemoTenants.Northside, "staff@northside.example", UserRole.Staff),
        SeedUser(hasher, DemoTenants.Northside, "volunteer@northside.example", UserRole.Volunteer),
        SeedUser(hasher, DemoTenants.Riverside, "admin@riverside.example", UserRole.Admin),
        SeedUser(hasher, DemoTenants.Riverside, "staff@riverside.example", UserRole.Staff),
        SeedUser(hasher, DemoTenants.Riverside, "volunteer@riverside.example", UserRole.Volunteer));

    await db.SaveChangesAsync();
}

static User SeedUser(PasswordHasher<User> hasher, Guid tenantId, string email, UserRole role)
{
    var user = new User { Id = Guid.NewGuid(), TenantId = tenantId, Email = email, Role = role };
    user.PasswordHash = hasher.HashPassword(user, "Demo123!");
    return user;
}

// Makes the top-level-statement Program class public so the isolation tests' (and any
// future integration tests') WebApplicationFactory<Program> can boot the real DI-wired
// host instead of a hand-rolled stand-in.
public partial class Program;
