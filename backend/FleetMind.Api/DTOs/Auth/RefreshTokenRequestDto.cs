namespace FleetMind.Api.DTOs.Auth;

/// <summary>
/// DTO for refresh token requests, primarily used by non-cookie clients (e.g., mobile).
/// Browser clients use the HttpOnly cookie instead of providing this.
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
