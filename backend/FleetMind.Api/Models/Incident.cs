using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Incident : BaseEntity
    {
        public Guid ShipId { get; set; }
        public Ship Ship { get; set; } = null!;

        public Guid? VoyageId { get; set; }
        public Voyage? Voyage { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Default set in config or DTO

        public Guid ReportedByUserId { get; set; }
        public User ReportedByUser { get; set; } = null!;

        public DateTime OccurredAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}
