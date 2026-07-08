using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

/// <summary>
/// Represents a user account in the FleetMind system.
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Unique email address used for authentication.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt or similar hash of the user's password. Never stores plaintext.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    
    // Account Lockout
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedOutUntil { get; set; }

    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Whether the user account is active. Inactive users cannot log in.
    /// </summary>
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;

    // ─── Navigation Properties ───────────────────────────────────────────────

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
