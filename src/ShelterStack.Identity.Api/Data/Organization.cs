namespace ShelterStack.Identity.Api.Data;

/// <summary>
/// The tenant itself (a shelter/rescue organization). Its <see cref="Id"/> is the tenant id
/// that <see cref="User.TenantId"/> and every other service's tenant-scoped rows refer to.
/// </summary>
public sealed class Organization
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
