using System.Collections.Generic;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Users;
using FluentValidation;

namespace FleetMind.Api.Validators.Users;

public class UpdateUserSettingsDtoValidator : AbstractValidator<UpdateUserSettingsDto>
{
    private static readonly HashSet<string> ValidThemes = new()
    {
        ThemePreference.Light,
        ThemePreference.Dark,
        ThemePreference.System
    };

    private static readonly HashSet<string> ValidNotificationTypes = new()
    {
        NotificationType.MaintenanceOverdue,
        NotificationType.VoyageDelayed,
        NotificationType.CertificationExpiring,
        NotificationType.FuelAnomaly,
        NotificationType.General,
        NotificationType.IncidentReported
    };

    public UpdateUserSettingsDtoValidator()
    {
        RuleFor(x => x.Theme)
            .NotEmpty().WithMessage("Theme is required.")
            .Must(t => ValidThemes.Contains(t)).WithMessage($"Theme must be one of: {string.Join(", ", ValidThemes)}");

        // We only reject genuinely malformed input here (e.g. non-boolean values if that were possible, 
        // though Dictionary<string, bool> enforces it structurally).
        // NOTE: Unrecognized keys should be IGNORED (not rejected) at the SERVICE layer when actually persisting.
        // This ensures clients with slightly-stale known preference keys don't get hard-rejected over one unrecognized key.
    }
}
