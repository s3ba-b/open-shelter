using Microsoft.AspNetCore.Http;
using ShelterStack.Identity.Api.Tenancy;
using Xunit;

namespace ShelterStack.Identity.Api.Tests;

public class HeaderTenantContextTests
{
    private static IHttpContextAccessor AccessorWithHeader(string? headerValue)
    {
        var httpContext = new DefaultHttpContext();
        if (headerValue is not null)
        {
            httpContext.Request.Headers[HeaderTenantContext.HeaderName] = headerValue;
        }

        return new HttpContextAccessor { HttpContext = httpContext };
    }

    [Fact]
    public void Resolves_TenantId_FromHeader()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new HeaderTenantContext(AccessorWithHeader(tenantId.ToString()));

        Assert.Equal(tenantId, tenantContext.TenantId);
    }

    [Fact]
    public void Throws_WhenHeaderMissing()
    {
        Assert.Throws<TenantResolutionException>(() => new HeaderTenantContext(AccessorWithHeader(null)));
    }

    [Fact]
    public void Throws_WhenHeaderIsNotAGuid()
    {
        Assert.Throws<TenantResolutionException>(() => new HeaderTenantContext(AccessorWithHeader("not-a-guid")));
    }

    [Fact]
    public void Throws_WhenNoHttpContext()
    {
        var accessor = new HttpContextAccessor { HttpContext = null };

        Assert.Throws<InvalidOperationException>(() => new HeaderTenantContext(accessor));
    }
}
