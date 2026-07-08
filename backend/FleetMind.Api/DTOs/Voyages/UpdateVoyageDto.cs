using System;

namespace FleetMind.Api.DTOs.Voyages
{
    public class UpdateVoyageDto
    {
        public Guid OriginPortId { get; set; }
        public Guid DestinationPortId { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public string? Notes { get; set; }
    }
}
