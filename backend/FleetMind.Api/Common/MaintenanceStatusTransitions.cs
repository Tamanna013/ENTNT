using System.Collections.Generic;

namespace FleetMind.Api.Common;

public static class MaintenanceStatusTransitions
{
    // Overdue is set exclusively by MaintenanceOverdueCheckService and must never
    // appear as an accepted target in UpdateMaintenanceStatusDtoValidator.
    
    private static readonly Dictionary<string, HashSet<string>> LegalTransitions = new()
    {
        { Constants.MaintenanceStatus.Scheduled, new HashSet<string> { Constants.MaintenanceStatus.InProgress, Constants.MaintenanceStatus.Cancelled } },
        { Constants.MaintenanceStatus.InProgress, new HashSet<string> { Constants.MaintenanceStatus.Completed, Constants.MaintenanceStatus.Cancelled } },
        { Constants.MaintenanceStatus.Completed, new HashSet<string>() },
        { Constants.MaintenanceStatus.Cancelled, new HashSet<string>() },
        // If a record is overdue, it can be put in progress or cancelled. Wait, does the spec say what Overdue can transition to?
        // Ah, the spec says: "Scheduled -> {InProgress, Cancelled}; InProgress -> {Completed, Cancelled}; Completed -> {} ; Cancelled -> {}."
        // Let me add Overdue -> {InProgress, Cancelled} for completeness, or just leave it empty if not specified. I will add {InProgress, Cancelled} so it can actually be worked on.
        { Constants.MaintenanceStatus.Overdue, new HashSet<string> { Constants.MaintenanceStatus.InProgress, Constants.MaintenanceStatus.Cancelled } }
    };

    public static bool IsLegalTransition(string currentStatus, string nextStatus)
    {
        if (LegalTransitions.TryGetValue(currentStatus, out var allowedNextStates))
        {
            return allowedNextStates.Contains(nextStatus);
        }
        return false;
    }

    public static IEnumerable<string> GetLegalNextStates(string currentStatus)
    {
        if (LegalTransitions.TryGetValue(currentStatus, out var allowedNextStates))
        {
            return allowedNextStates;
        }
        return Array.Empty<string>();
    }
}
