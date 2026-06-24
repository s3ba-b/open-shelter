using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ShelterStack.Web.Auth;

/// <summary>
/// Holds the authenticated session for one Blazor circuit: the raw access token (which
/// <see cref="GatewayClient"/> attaches as a Bearer header on outbound Gateway calls) and the
/// <see cref="ClaimsPrincipal"/> the UI authorizes against. Registered scoped, so each user's
/// circuit has its own isolated session; there is no refresh flow in M1, so a full page reload
/// starts a new (signed-out) circuit and the user signs in again.
/// </summary>
public sealed class WebAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private ClaimsPrincipal _user = Anonymous;

    /// <summary>The current access token, or <c>null</c> when signed out.</summary>
    public string? AccessToken { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(_user));

    /// <summary>
    /// Establishes the session from a freshly issued access token. Tenant, role, and the
    /// organization name come from the token's claims — never from client-supplied input; the
    /// <paramref name="email"/> is display-only, the credential the user just authenticated
    /// with, since the token's subject is an opaque user id.
    /// </summary>
    public void SignIn(string accessToken, string email)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        // Re-key the token's claims onto an identity whose name/role types the UI understands,
        // so AuthorizeView and User.IsInRole("Admin"/"Staff") work off the "role" claim.
        var claims = token.Claims.ToList();
        claims.Add(new Claim(TokenClaims.Email, email));

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "jwt",
            nameType: TokenClaims.Email,
            roleType: TokenClaims.Role
        );

        AccessToken = accessToken;
        _user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }

    /// <summary>Clears the session so protected routes redirect back to sign-in.</summary>
    public void SignOut()
    {
        AccessToken = null;
        _user = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }
}
