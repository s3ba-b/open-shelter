namespace OpenShelter.Animals.Api.Data;

/// <summary>
/// A tenant-scoped animal record — the first real domain entity of M2 (the animals
/// vertical slice). <see cref="TenantId"/> and the EF Core global query filter built on it
/// (see <see cref="AnimalsDbContext"/>) carry the project's non-negotiable isolation rule and
/// are unchanged from the M0 placeholder. Status tracking and intake history are separate
/// entities in this milestone that hang off this one.
/// </summary>
public sealed class Animal
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public AnimalSpecies Species { get; set; }

    /// <summary>Optional breed, free text (e.g. "Labrador Retriever"); unknown for many strays.</summary>
    public string? Breed { get; set; }

    public AnimalSex Sex { get; set; }

    /// <summary>
    /// Date of birth when known. Often only approximate for shelter intakes, so it is nullable;
    /// callers that only know an approximate age can store the estimated birth date or leave it unset.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Free-text notes — temperament, markings, medical flags, etc.</summary>
    public string? Description { get; set; }
}
