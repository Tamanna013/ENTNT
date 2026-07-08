using System.Collections.Generic;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Common
{
    public static class IncidentStatusTransitions
    {
        private static readonly Dictionary<string, string[]> LegalTransitions = new()
        {
            { IncidentStatus.Reported, new[] { IncidentStatus.UnderInvestigation, IncidentStatus.Closed } },
            { IncidentStatus.UnderInvestigation, new[] { IncidentStatus.Resolved, IncidentStatus.Closed } },
            { IncidentStatus.Resolved, new[] { IncidentStatus.Closed } },
            { IncidentStatus.Closed, System.Array.Empty<string>() }
        };

        public static bool IsLegalTransition(string currentStatus, string nextStatus)
        {
            if (currentStatus == nextStatus) return true;
            
            if (LegalTransitions.TryGetValue(currentStatus, out var allowedStates))
            {
                foreach (var state in allowedStates)
                {
                    if (state == nextStatus) return true;
                }
            }
            return false;
        }

        public static string[] GetLegalNextStates(string currentStatus)
        {
            if (LegalTransitions.TryGetValue(currentStatus, out var allowedStates))
            {
                return allowedStates;
            }
            return System.Array.Empty<string>();
        }
    }
}
