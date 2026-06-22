namespace ShelterStack.Animals.Api.Data;

/// <summary>
/// An animal's sex. <see cref="Unknown"/> is the default because it is often not
/// established at intake. Persisted as its string name (see <see cref="AnimalsDbContext"/>).
/// </summary>
public enum AnimalSex
{
    Unknown,
    Male,
    Female,
}
