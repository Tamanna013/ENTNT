using System;
using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Users;

public class UserSettingsDto
{
    public string Theme { get; set; } = string.Empty;
    public Dictionary<string, bool> NotificationPreferences { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}
