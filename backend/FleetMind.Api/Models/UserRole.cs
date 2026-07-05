namespace FleetMind.Api.Models;

/// <summary>
/// Many-to-many join entity linking Users to Roles.
/// Does NOT inherit BaseEntity — this is a pure relationship table with a composite key.
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
