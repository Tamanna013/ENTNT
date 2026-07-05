namespace FleetMind.Api.DTOs.Users;

/// <summary>
/// Input DTO for updating an existing user's profile.
/// Email and Password changes are handled through separate dedicated endpoints.
/// </summary>
public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public List<string> RoleNames { get; set; } = new();
}
