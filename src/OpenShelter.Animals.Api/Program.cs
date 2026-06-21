using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenShelter.Animals.Api.Auth;
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

// Validate the JWT bearer tokens issued by OpenShelter.Identity.Api against the same signing
// key/issuer/audience (the "Jwt" section). Configure<JwtOptions> also exposes the values to
// integration tests via IOptions; the local snapshot is what AddJwtBearer needs at startup.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep the custom claims ("role", "tenant_id") under their original names instead of
        // remapping "role" to the long ClaimTypes.Role URI — matches how the tokens are issued,
        // and RoleClaimType below points RequireRole at that same "role" claim.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            RoleClaimType = TokenAuth.RoleClaim,
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy(TokenAuth.StaffOrAdminPolicy, policy =>
        policy.RequireRole(TokenAuth.AdminRole, TokenAuth.StaffRole)));

// Tenant resolution now comes from the authenticated token's tenant_id claim (replacing the
// M0 X-Tenant-Id header). The ITenantContext contract and every query filter built on it are
// unchanged — only the source of the tenant id moved.
builder.Services.AddScoped<ITenantContext, ClaimsTenantContext>();

var app = builder.Build();

app.MapDefaultEndpoints();

await SeedDemoTenantsAsync(app.Services);

app.UseAuthentication();
app.UseAuthorization();

// Trivial liveness/ping endpoint (anonymous), reachable through the gateway at /animals/ping.
app.MapGet("/ping", () => Results.Ok(new { service = "animals-api", status = "ok" }));

// Lists animals for the tenant resolved from the authenticated caller's token, demonstrating
// that the EF Core global query filter scopes results without any explicit per-call filtering.
// Restricted to admins and staff as a first example of role enforcement (volunteers get 403);
// finer-grained policies arrive with the domain endpoints in later milestones. Reachable
// through the gateway at /animals.
app.MapGet("/", async (AnimalsDbContext db) =>
    Results.Ok(await db.Animals.Select(a => new { a.Id, a.Name }).ToListAsync()))
    .RequireAuthorization(TokenAuth.StaffOrAdminPolicy);

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
