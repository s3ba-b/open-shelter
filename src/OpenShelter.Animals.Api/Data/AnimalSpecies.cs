namespace OpenShelter.Animals.Api.Data;

/// <summary>
/// The kind of animal a shelter intakes. Persisted as its string name (see
/// <see cref="AnimalsDbContext"/>) so the stored value stays readable and survives
/// reordering of the enum.
/// </summary>
public enum AnimalSpecies
{
    Dog,
    Cat,
    Rabbit,
    Bird,
    Other,
}
