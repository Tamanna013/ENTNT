using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

/// <summary>
/// Represents a persistent refresh token for an authenticated user.
/// Stored securely as a hash, not plaintext.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// When a token is rotated, this points to the hash of the new token that replaced it,
    /// forming an audit chain to detect replay attacks.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsExpired && !IsRevoked;
}
