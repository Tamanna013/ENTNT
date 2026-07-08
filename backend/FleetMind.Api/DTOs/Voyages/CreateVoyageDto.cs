using System;

namespace FleetMind.Api.DTOs.Voyages
{
    public class CreateVoyageDto
    {
        public Guid ShipId { get; set; }
        public string VoyageNumber { get; set; } = string.Empty;
        public Guid OriginPortId { get; set; }
        public Guid DestinationPortId { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public string? Notes { get; set; }
    }
}
