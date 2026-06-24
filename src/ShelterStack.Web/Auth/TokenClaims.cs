namespace ShelterStack.Web.Auth;

/// <summary>
/// Names of the claims carried by the access tokens issued by ShelterStack.Identity.Api
/// (mirroring its <c>JwtTokenIssuer</c>). The web app reads these for display and auth state;
/// the authoritative validation of <c>tenant_id</c>/<c>role</c> happens at the Gateway APIs.
/// </summary>
public static class TokenClaims
{
    public const string TenantId = "tenant_id";
    public const string Role = "role";
    public const string OrgName = "org_name";

    /// <summary>
    /// Display-only email of the signed-in user. It is not minted into the token (the token's
    /// <c>sub</c> is the user id); the sign-in flow attaches it from the credential the user
    /// just authenticated with so the shell can name the user.
    /// </summary>
    public const string Email = "email";
}
