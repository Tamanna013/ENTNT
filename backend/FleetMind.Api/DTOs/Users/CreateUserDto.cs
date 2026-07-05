namespace FleetMind.Api.DTOs.Users;

/// <summary>
/// Input DTO for creating a new user account.
/// Password is accepted as plaintext here — hashing occurs in the service layer.
/// </summary>
public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<string> RoleNames { get; set; } = new();
}
