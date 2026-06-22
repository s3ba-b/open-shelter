using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ShelterStack.Identity.Api.Data;
using ShelterStack.Identity.Api.Tenancy;
using Testcontainers.PostgreSql;
using Xunit;

namespace ShelterStack.Identity.Api.IsolationTests;

/// <summary>
/// Drives the real Identity API host (Program.cs, unchanged) over HTTP against a real
/// Postgres container, asserting that a request resolved to one tenant never sees another
/// tenant's users. See CHARTER.md's cross-tenant risk mitigation for why this goes through
/// the actual DI-wired host rather than constructing IdentityDbContext directly.
/// </summary>
public sealed class CrossTenantIsolationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("identitydb")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Program.cs's AddNpgsqlDataSource resolves "ConnectionStrings:identitydb" lazily
        // (the first time the data source is requested, during startup seeding) rather than
        // at configuration-build time, so the env var just needs to be in place before that —
        // ConfigureAppConfiguration on WithWebHostBuilder doesn't reliably layer over a
        // ConfigurationManager already populated by WebApplication.CreateBuilder for the
        // minimal hosting model.
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__identitydb",
            _postgres.GetConnectionString()
        );

        // Run as Production, not the WebApplicationFactory default of Development: ASP.NET
        // Core's DI scope-validation (on by default in Development) would turn a pooled-vs-
        // scoped DbContext regression into a startup crash, masking the actual failure mode —
        // a silent cross-tenant data leak — that this test exists to catch. Per CHARTER.md,
        // that crash isn't a safety net we get in Production, so the test shouldn't lean on it.
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
            b.UseEnvironment("Production")
        );
    }

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await _postgres.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__identitydb", null);
    }

    [Fact]
    public async Task TenantRequests_OnlySeeTheirOwnUsers()
    {
        using var client = _factory.CreateClient();

        var northsideUsers = await GetUsersAsync(client, DemoTenants.Northside);
        Assert.Equal(3, northsideUsers.Length);
        Assert.All(northsideUsers, u => Assert.EndsWith("@northside.example", u.Email));

        // Same HttpClient/host as the Northside call above: a DbContext registered pooled
        // (AddDbContextPool) instead of scoped (AddDbContext) would hand this request the
        // pooled instance still bound to Northside's tenant context, returning Northside's
        // users again instead of Riverside's own users.
        var riversideUsers = await GetUsersAsync(client, DemoTenants.Riverside);
        Assert.Equal(3, riversideUsers.Length);
        Assert.All(riversideUsers, u => Assert.EndsWith("@riverside.example", u.Email));
        Assert.DoesNotContain(riversideUsers, u => u.Email.EndsWith("@northside.example"));
    }

    private static async Task<UserDto[]> GetUsersAsync(HttpClient client, Guid tenantId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/users");
        request.Headers.Add(HeaderTenantContext.HeaderName, tenantId.ToString());

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserDto[]>() ?? [];
    }

    private sealed record UserDto(Guid Id, string Email, UserRole Role);
}
