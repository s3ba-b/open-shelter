namespace ShelterStack.Identity.Api.Tenancy;

/// <summary>
/// Placeholder tenant resolution, same role as <c>ShelterStack.Animals.Api</c>'s context of the
/// same name: reads the tenant id from a request header. M1's auth pipeline replaces this with
/// resolution from an authenticated tenant claim; the <see cref="ITenantContext"/> contract and
/// everything built on it (query filters, seeded data) stays unchanged.
/// </summary>
public sealed class HeaderTenantContext : ITenantContext
{
    public const string HeaderName = "X-Tenant-Id";

    public HeaderTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext =
            httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "Tenant resolution requires an active HTTP request."
            );

        var headerValue = httpContext.Request.Headers[HeaderName].FirstOrDefault();
        if (!Guid.TryParse(headerValue, out var tenantId))
        {
            throw new TenantResolutionException(
                $"Request is missing a valid '{HeaderName}' header (expected a GUID)."
            );
        }

        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}

public sealed class TenantResolutionException(string message) : Exception(message);
