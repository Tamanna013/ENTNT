using System;

namespace FleetMind.Api.DTOs.Voyages
{
    public class VoyageDto
    {
        public Guid Id { get; set; }
        public Guid ShipId { get; set; }
        public string ShipName { get; set; } = string.Empty;
        public string VoyageNumber { get; set; } = string.Empty;
        public Guid OriginPortId { get; set; }
        public string OriginPortName { get; set; } = string.Empty;
        public Guid DestinationPortId { get; set; }
        public string DestinationPortName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public DateTime? ActualArrivalDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
