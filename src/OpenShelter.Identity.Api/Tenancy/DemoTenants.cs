namespace OpenShelter.Identity.Api.Tenancy;

/// <summary>
/// Same ids as <c>OpenShelter.Animals.Api</c>'s <c>DemoTenants</c> so the two services'
/// seeded demo data refers to the same two tenants.
/// </summary>
public static class DemoTenants
{
    public static readonly Guid Northside = new("11111111-1111-1111-1111-111111111111");

    public static readonly Guid Riverside = new("22222222-2222-2222-2222-222222222222");
}
