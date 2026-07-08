using System;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Voyage : BaseEntity
    {
        public Guid ShipId { get; set; }
        public Ship Ship { get; set; } = null!;

        public string VoyageNumber { get; set; } = string.Empty;
        
        public Guid OriginPortId { get; set; }
        public Port? OriginPort { get; set; }
        public Guid DestinationPortId { get; set; }
        public Port? DestinationPort { get; set; }
        
        public DateTime DepartureDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public DateTime? ActualArrivalDate { get; set; }
        
        public string Status { get; set; } = VoyageStatus.Scheduled;
        
        public string? Notes { get; set; }

        public ICollection<Cargo> CargoItems { get; set; } = new List<Cargo>();
        public ICollection<Container> Containers { get; set; } = new List<Container>();
        public ICollection<FuelLog> FuelLogs { get; set; } = new List<FuelLog>();
        public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    }
}
