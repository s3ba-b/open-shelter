namespace ShelterStack.Identity.Api.Auth;

/// <summary>
/// Configuration for the access tokens the login endpoint issues, bound from the "Jwt"
/// configuration section. The signing key in appsettings.json is a development/demo key
/// (consistent with the committed demo passwords) and must be overridden via a secret in
/// any real deployment.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Access tokens are short-lived; there is no refresh flow in M1.</summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
}
