namespace OpenShelter.Animals.Api.Data;

/// <summary>
/// Minimal tenant-scoped entity for M0: proves the isolation mechanism end-to-end.
/// M2 extends this with the full animal domain (intake history, status tracking, etc).
/// </summary>
public sealed class Animal
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
}
