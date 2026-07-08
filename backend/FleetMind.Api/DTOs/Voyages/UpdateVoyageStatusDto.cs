using System;

namespace FleetMind.Api.DTOs.Voyages
{
    public class UpdateVoyageStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? ActualArrivalDate { get; set; }
    }
}
