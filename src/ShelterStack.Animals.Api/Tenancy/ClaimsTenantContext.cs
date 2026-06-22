using ShelterStack.Animals.Api.Auth;

namespace ShelterStack.Animals.Api.Tenancy;

/// <summary>
/// Resolves the tenant from the authenticated caller's validated <c>tenant_id</c> claim,
/// replacing the M0 <c>X-Tenant-Id</c> header placeholder. Authentication runs before the
/// endpoint, so by the time a tenant-scoped <see cref="ITenantContext"/> is resolved the
/// principal is present and the claim is trustworthy. The contract and every query filter
/// built on it stay unchanged — only the source of the tenant id moves from header to token.
/// </summary>
public sealed class ClaimsTenantContext : ITenantContext
{
    public ClaimsTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        var user =
            httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException(
                "Tenant resolution requires an active HTTP request."
            );

        var tenantClaim = user.FindFirst(TokenAuth.TenantIdClaim)?.Value;
        if (!Guid.TryParse(tenantClaim, out var tenantId))
        {
            throw new InvalidOperationException(
                $"Authenticated token is missing a valid '{TokenAuth.TenantIdClaim}' claim (expected a GUID)."
            );
        }

        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}
