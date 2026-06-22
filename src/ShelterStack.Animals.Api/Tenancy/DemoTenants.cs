namespace ShelterStack.Animals.Api.Tenancy;

/// <summary>
/// Fixed ids for the two demo tenants seeded at startup (M0 acceptance criteria),
/// so the cross-tenant isolation test (next issue) can target them directly
/// instead of discovering ids at runtime.
/// </summary>
public static class DemoTenants
{
    public static readonly Guid Northside = new("11111111-1111-1111-1111-111111111111");

    public static readonly Guid Riverside = new("22222222-2222-2222-2222-222222222222");
}
