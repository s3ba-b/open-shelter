using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShelterStack.Identity.Api.Auth;
using ShelterStack.Identity.Api.Tenancy;
using Testcontainers.PostgreSql;
using Xunit;

namespace ShelterStack.Identity.Api.IntegrationTests;

/// <summary>
/// Exercises <c>POST /login</c> through the real Identity API host over HTTP against a real
/// Postgres container, against the seeded demo users: valid credentials yield a 200 with a
/// JWT carrying the user's tenant and role, bad credentials yield 401, and the issued token
/// validates against the host's configured signing key.
/// </summary>
public sealed class LoginEndpointTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("identitydb")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // See CrossTenantIsolationTests for why the connection string is supplied via env var
        // and the host runs as Production.
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__identitydb",
            _postgres.GetConnectionString()
        );
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
    public async Task ValidCredentials_Return200WithTenantAndRoleClaims()
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/login",
            new LoginRequest("admin@northside.example", "Demo123!")
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));

        // Validate the token against the host's own configured signing key (criterion 3):
        // decode and verify the signature/issuer/audience/lifetime rather than just trusting
        // the response shape.
        var principal = ValidateToken(body.AccessToken);
        Assert.Equal(
            DemoTenants.Northside.ToString(),
            principal.FindFirst(JwtTokenIssuer.TenantIdClaim)?.Value
        );
        Assert.Equal("Admin", principal.FindFirst(JwtTokenIssuer.RoleClaim)?.Value);
    }

    [Theory]
    [InlineData("admin@northside.example", "wrong-password")]
    [InlineData("nobody@northside.example", "Demo123!")]
    public async Task InvalidCredentials_Return401(string email, string password)
    {
        using var client = _factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/login",
            new LoginRequest(email, password)
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private System.Security.Claims.ClaimsPrincipal ValidateToken(string token)
    {
        var jwt = _factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;

        var handler = new JwtSecurityTokenHandler
        {
            // Keep custom claims ("role", "tenant_id") under their original names instead of
            // remapping "role" to the long ClaimTypes.Role URI.
            MapInboundClaims = false,
        };

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
        };

        return handler.ValidateToken(token, parameters, out _);
    }
}
