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
    string? Description)
{
    public static AnimalResponse From(Animal animal) => new(
        animal.Id,
        animal.Name,
        animal.Species,
        animal.Breed,
        animal.Sex,
        animal.DateOfBirth,
        animal.Description);
}
