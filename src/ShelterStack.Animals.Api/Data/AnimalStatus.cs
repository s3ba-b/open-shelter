namespace ShelterStack.Animals.Api.Data;

/// <summary>
/// Where an animal sits in its shelter lifecycle. <see cref="Intake"/> is the default for a
/// newly created animal. Legal moves between these are enforced by
/// <see cref="AnimalStatusTransitions"/>, not by this enum itself. Persisted as its string name
/// (see <see cref="AnimalsDbContext"/>).
/// </summary>
public enum AnimalStatus
{
    Intake,
    Available,
    Adopted,
    Fostered,
    MedicalHold,
    Returned,
}
