namespace ShelterStack.Identity.Api.Tenancy;

/// <summary>
/// Resolves the tenant the current request is scoped to. Registered per-request (scoped)
/// so every EF Core query filter and downstream service sees a single, consistent tenant.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
}
