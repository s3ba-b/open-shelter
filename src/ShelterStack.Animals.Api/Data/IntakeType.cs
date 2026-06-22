namespace ShelterStack.Animals.Api.Data;

/// <summary>
/// How an animal came to the shelter for a given intake. Persisted as its string name (see
/// <see cref="AnimalsDbContext"/>), same pattern as <see cref="AnimalStatus"/>.
/// </summary>
public enum IntakeType
{
    Stray,
    OwnerSurrender,
    TransferIn,
    Other,
}
