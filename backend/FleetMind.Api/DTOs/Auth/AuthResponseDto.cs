using FleetMind.Api.DTOs.Users;

namespace FleetMind.Api.DTOs.Auth;

/// <summary>
/// Response DTO returned on successful registration or login.
/// Contains the JWT access token, its expiry, and the authenticated user's info.
/// </summary>
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}
