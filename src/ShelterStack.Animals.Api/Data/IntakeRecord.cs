namespace ShelterStack.Animals.Api.Data;

/// <summary>
/// One recorded intake of an animal into the shelter. Tenant-scoped exactly like
/// <see cref="Animal"/> and <see cref="AnimalStatusHistory"/> — see the global query filter in
/// <see cref="AnimalsDbContext"/> — and append-only: an animal can have more than one of these
/// over its life (e.g. returned, then re-intaken).
/// </summary>
public sealed class IntakeRecord
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AnimalId { get; set; }

    public DateOnly IntakeDate { get; set; }

    public IntakeType IntakeType { get; set; }

    /// <summary>Free-text notes — e.g. where a stray was found, or who surrendered the animal.</summary>
    public string? Notes { get; set; }
}
