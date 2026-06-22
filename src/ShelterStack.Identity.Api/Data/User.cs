namespace ShelterStack.Identity.Api.Data;

/// <summary>
/// A user within an organization. The first tenant-scoped entity in the Identity service,
/// so it carries the global query filter (see <see cref="IdentityDbContext"/>) that
/// <c>ShelterStack.Animals.Api</c>'s <c>Animal</c> entity established the pattern for.
/// </summary>
public sealed class User
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}
