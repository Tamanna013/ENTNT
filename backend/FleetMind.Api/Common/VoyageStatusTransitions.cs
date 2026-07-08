using System.Collections.Generic;
using System.Linq;

namespace FleetMind.Api.Common
{
    public static class VoyageStatusTransitions
    {
        private static readonly Dictionary<string, HashSet<string>> _validTransitions = new(System.StringComparer.OrdinalIgnoreCase)
        {
            { Constants.VoyageStatus.Scheduled, new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { Constants.VoyageStatus.InTransit, Constants.VoyageStatus.Cancelled, Constants.VoyageStatus.Delayed } },
            { Constants.VoyageStatus.Delayed, new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { Constants.VoyageStatus.InTransit, Constants.VoyageStatus.Cancelled } },
            { Constants.VoyageStatus.InTransit, new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { Constants.VoyageStatus.Completed, Constants.VoyageStatus.Delayed } },
            { Constants.VoyageStatus.Completed, new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) },
            { Constants.VoyageStatus.Cancelled, new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) }
        };

        public static bool IsLegalTransition(string currentStatus, string targetStatus)
        {
            if (string.Equals(currentStatus, targetStatus, System.StringComparison.OrdinalIgnoreCase))
                return true; // same state is trivial

            if (_validTransitions.TryGetValue(currentStatus, out var legalNextStates))
            {
                return legalNextStates.Contains(targetStatus);
            }
            return false;
        }

        public static IEnumerable<string> GetLegalNextStates(string currentStatus)
        {
            if (_validTransitions.TryGetValue(currentStatus, out var legalNextStates))
            {
                return legalNextStates;
            }
            return Enumerable.Empty<string>();
        }
    }
}
