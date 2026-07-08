using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class ContainerTrackingEvent : BaseEntity
    {
        public Guid ContainerId { get; set; }
        public Container Container { get; set; } = null!;

        public string EventType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
        
        public Guid RecordedByUserId { get; set; }
    }
}
