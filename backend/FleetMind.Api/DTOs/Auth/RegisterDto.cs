namespace FleetMind.Api.DTOs.Auth;

/// <summary>
/// Input DTO for user self-registration.
/// No RoleNames field — self-registration always assigns the default "User" role.
/// </summary>
public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
