namespace OpenShelter.Identity.Api.Auth;

/// <summary>Credentials posted to <c>POST /login</c>.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>The signed access token returned on a successful login.</summary>
public sealed record LoginResponse(string AccessToken);
