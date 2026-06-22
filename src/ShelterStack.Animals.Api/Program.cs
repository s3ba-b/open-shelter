using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ShelterStack.Animals.Api;
using ShelterStack.Animals.Api.Auth;
using ShelterStack.Animals.Api.Data;
using ShelterStack.Animals.Api.Tenancy;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Serialize the Species/Sex enums as their names ("Dog", "Female") rather than integers, so
// the resource shape stays readable and decoupled from the enum's declaration order.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Connect to the PostgreSQL "shelterstackdb" resource via Aspire service discovery,
// registering an NpgsqlDataSource plus a health check that proves the connection.
builder.AddNpgsqlDataSource("shelterstackdb");

// Unpooled by design: AnimalsDbContext takes the per-request ITenantContext as a
// constructor dependency, and DbContext pooling reuses instances (and whatever
// scoped service they captured) across unrelated requests' scopes — exactly the
// kind of cross-tenant leak this milestone exists to prevent.
builder.Services.AddDbContext<AnimalsDbContext>((sp, options) =>
    options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddHttpContextAccessor();

// Validate the JWT bearer tokens issued by ShelterStack.Identity.Api against the same signing
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

// Tenant-scoped animal CRUD. Every route is restricted to admins and staff (volunteers get
// 403), and every query rides the EF Core global query filter, so a caller only ever reads or
// writes their own tenant's animals — no explicit per-call TenantId filtering. Reachable
// through the gateway under /animals.
var animals = app.MapGroup("").RequireAuthorization(TokenAuth.StaffOrAdminPolicy);

// List the caller's animals.
animals.MapGet("/", async (AnimalsDbContext db) =>
{
    var results = await db.Animals
        .OrderBy(a => a.Name)
        .Select(a => AnimalResponse.From(a))
        .ToListAsync();

    return Results.Ok(results);
});

// Fetch one animal by id. The query filter turns a cross-tenant id into a 404, exactly as if
// the row did not exist — another tenant's animal is never distinguishable from a missing one.
animals.MapGet("/{id:guid}", async (Guid id, AnimalsDbContext db) =>
{
    var animal = await db.Animals.FirstOrDefaultAsync(a => a.Id == id);

    return animal is null ? Results.NotFound() : Results.Ok(AnimalResponse.From(animal));
});

// Create an animal in the caller's tenant. TenantId comes from the resolved ITenantContext
// (the token), never the request body, so a caller cannot plant a row in another tenant.
animals.MapPost("/", async (CreateAnimalRequest request, AnimalsDbContext db, ITenantContext tenant) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Name)] = ["Name is required."],
        });
    }

    var animal = new Animal
    {
        Id = Guid.NewGuid(),
        TenantId = tenant.TenantId,
        Name = request.Name,
        Species = request.Species,
        Breed = request.Breed,
        Sex = request.Sex,
        DateOfBirth = request.DateOfBirth,
        Description = request.Description,
    };

    db.Animals.Add(animal);
    await db.SaveChangesAsync();

    return Results.Created($"/{animal.Id}", AnimalResponse.From(animal));
});

// Update an animal. The query filter scopes the lookup to the caller's tenant, so an attempt
// to update another tenant's animal resolves to NotFound rather than mutating their data.
animals.MapPut("/{id:guid}", async (Guid id, UpdateAnimalRequest request, AnimalsDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Name)] = ["Name is required."],
        });
    }

    var animal = await db.Animals.FirstOrDefaultAsync(a => a.Id == id);
    if (animal is null)
    {
        return Results.NotFound();
    }

    animal.Name = request.Name;
    animal.Species = request.Species;
    animal.Breed = request.Breed;
    animal.Sex = request.Sex;
    animal.DateOfBirth = request.DateOfBirth;
    animal.Description = request.Description;

    await db.SaveChangesAsync();

    return Results.Ok(AnimalResponse.From(animal));
});

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
        new Animal
        {
            Id = Guid.NewGuid(),
            TenantId = DemoTenants.Northside,
            Name = "Buddy",
            Species = AnimalSpecies.Dog,
            Breed = "Labrador Retriever",
            Sex = AnimalSex.Male,
            DateOfBirth = new DateOnly(2021, 4, 12),
            Description = "Friendly, house-trained; good with children.",
        },
        new Animal
        {
            Id = Guid.NewGuid(),
            TenantId = DemoTenants.Riverside,
            Name = "Whiskers",
            Species = AnimalSpecies.Cat,
            Breed = "Domestic Shorthair",
            Sex = AnimalSex.Female,
            DateOfBirth = new DateOnly(2022, 9, 1),
            Description = "Shy at first; prefers a quiet home.",
        });

    await db.SaveChangesAsync();
}

// Makes the top-level-statement Program class public so the isolation tests' (and any
// future integration tests') WebApplicationFactory<Program> can boot the real DI-wired
// host instead of a hand-rolled stand-in.
public partial class Program;
