namespace ShelterStack.Animals.Api.Data;

/// <summary>
/// The allowed-transitions policy for <see cref="AnimalStatus"/>: which moves a status-change
/// request may legally make. Adoption can only be reversed via <see cref="AnimalStatus.Returned"/>
/// (no jumping straight back to <see cref="AnimalStatus.Intake"/>), which is the specific illegal
/// move called out in the M2 status-tracking issue.
/// </summary>
public static class AnimalStatusTransitions
{
    private static readonly Dictionary<AnimalStatus, AnimalStatus[]> Allowed = new()
    {
        [AnimalStatus.Intake] = [AnimalStatus.Available, AnimalStatus.MedicalHold],
        [AnimalStatus.Available] =
        [
            AnimalStatus.Adopted,
            AnimalStatus.Fostered,
            AnimalStatus.MedicalHold,
        ],
        [AnimalStatus.Fostered] =
        [
            AnimalStatus.Available,
            AnimalStatus.Adopted,
            AnimalStatus.MedicalHold,
        ],
        [AnimalStatus.MedicalHold] = [AnimalStatus.Available, AnimalStatus.Intake],
        [AnimalStatus.Adopted] = [AnimalStatus.Returned],
        [AnimalStatus.Returned] = [AnimalStatus.Intake, AnimalStatus.Available],
    };

    public static bool IsAllowed(AnimalStatus from, AnimalStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}
