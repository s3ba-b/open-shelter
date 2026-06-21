namespace OpenShelter.Animals.Api.Auth;

/// <summary>
/// Names of the custom claims carried by the access tokens issued by OpenShelter.Identity.Api
/// (mirroring its <c>JwtTokenIssuer</c>), the role values they hold, and the authorization
/// policies built on them.
/// </summary>
public static class TokenAuth
{
    public const string TenantIdClaim = "tenant_id";
    public const string RoleClaim = "role";

    public const string AdminRole = "Admin";
    public const string StaffRole = "Staff";

    /// <summary>
    /// First example of role enforcement: shelter admins and staff, but not volunteers.
    /// Finer-grained, per-endpoint policies arrive with the domain endpoints in later milestones.
    /// </summary>
    public const string StaffOrAdminPolicy = "StaffOrAdmin";
}
