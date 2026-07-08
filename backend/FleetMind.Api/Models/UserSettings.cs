using System;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class UserSettings : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Theme { get; set; } = ThemePreference.System;

    // We store this as JSON in the database instead of a fully normalized 
    // separate table, since this data is always read/written as one whole unit 
    // per user and never filtered at the individual-preference level by any current feature.
    public string NotificationPreferencesJson { get; set; } = "{}";
}
