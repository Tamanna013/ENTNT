using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class EmailVerificationToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
