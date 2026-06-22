using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShelterStack.Identity.Api.Data;

namespace ShelterStack.Identity.Api.Auth;

/// <summary>
/// Issues signed JWT access tokens carrying the claims the rest of the system trusts in
/// place of the M0 placeholder <c>X-Tenant-Id</c> header: the user's id (<c>sub</c>), their
/// <c>tenant_id</c>, and their <c>role</c>. Wiring other services to validate these tokens
/// is the next issue; here we only mint them.
/// </summary>
public sealed class JwtTokenIssuer(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public const string TenantIdClaim = "tenant_id";
    public const string RoleClaim = "role";

    public string IssueAccessToken(User user)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(TenantIdClaim, user.TenantId.ToString()),
            new Claim(RoleClaim, user.Role.ToString()),
        };

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_options.AccessTokenLifetimeMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
