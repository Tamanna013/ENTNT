using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

/// <summary>
/// Represents a security role in the FleetMind system.
/// Specific role names (Admin, Captain, etc.) are seeded via data migrations, not hardcoded here.
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // ─── Navigation Properties ───────────────────────────────────────────────

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
