using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Users;

public class UpdateUserSettingsDto
{
    public string Theme { get; set; } = string.Empty;
    public Dictionary<string, bool> NotificationPreferences { get; set; } = new();
}
