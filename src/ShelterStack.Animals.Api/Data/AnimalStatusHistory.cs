namespace ShelterStack.Animals.Api.Data;

/// <summary>
/// One recorded status change for an animal. Tenant-scoped exactly like <see cref="Animal"/> —
/// see the global query filter in <see cref="AnimalsDbContext"/> — and append-only: rows are
/// never updated or deleted, they are the audit trail the status-change endpoint writes to.
/// </summary>
public sealed class AnimalStatusHistory
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AnimalId { get; set; }

    public AnimalStatus Status { get; set; }

    public DateTimeOffset ChangedAtUtc { get; set; }
}
