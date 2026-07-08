using System;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Ship : BaseEntity
    {
        public Guid FleetId { get; set; }
        public Fleet Fleet { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string IMO { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Status { get; set; } = ShipStatus.Active;
        public int YearBuilt { get; set; }
        public decimal GrossTonnage { get; set; }
        public string Flag { get; set; } = null!;

        public Guid? PrimaryPhotoAttachmentId { get; set; }
        public Attachment? PrimaryPhotoAttachment { get; set; }

        public ICollection<CrewMember> CrewMembers { get; set; } = new List<CrewMember>();
        public ICollection<Voyage> Voyages { get; set; } = new List<Voyage>();
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<FuelLog> FuelLogs { get; set; } = new List<FuelLog>();
        public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    }
}
