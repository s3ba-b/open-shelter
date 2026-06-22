using ShelterStack.Animals.Api.Data;

namespace ShelterStack.Animals.Api;

/// <summary>Fields accepted when creating an animal. <c>TenantId</c> is deliberately absent —
/// it is taken from the caller's authenticated token, never the request body.</summary>
public sealed record CreateAnimalRequest(
    string Name,
    AnimalSpecies Species,
    string? Breed,
    AnimalSex Sex,
    DateOnly? DateOfBirth,
    string? Description);

/// <summary>Fields accepted when updating an animal (full replace of the editable fields).</summary>
public sealed record UpdateAnimalRequest(
    string Name,
    AnimalSpecies Species,
    string? Breed,
    AnimalSex Sex,
    DateOnly? DateOfBirth,
    string? Description);

/// <summary>The resource shape returned by the read and write endpoints.</summary>
public sealed record AnimalResponse(
    Guid Id,
    string Name,
    AnimalSpecies Species,
    string? Breed,
    AnimalSex Sex,
    DateOnly? DateOfBirth,
    string? Description,
    AnimalStatus Status)
{
    public static AnimalResponse From(Animal animal) => new(
        animal.Id,
        animal.Name,
        animal.Species,
        animal.Breed,
        animal.Sex,
        animal.DateOfBirth,
        animal.Description,
        animal.Status);
}

/// <summary>The status a caller wants to move an animal to. Rejected with a 4xx by the
/// status-change endpoint if it isn't a legal move from the animal's current status — see
/// <see cref="AnimalStatusTransitions"/>.</summary>
public sealed record ChangeAnimalStatusRequest(AnimalStatus Status);

/// <summary>One row of an animal's status-change history, as returned by the
/// list-status-history endpoint.</summary>
public sealed record AnimalStatusHistoryResponse(Guid Id, AnimalStatus Status, DateTimeOffset ChangedAtUtc)
{
    public static AnimalStatusHistoryResponse From(AnimalStatusHistory history) => new(
        history.Id,
        history.Status,
        history.ChangedAtUtc);
}
