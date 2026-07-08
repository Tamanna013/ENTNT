using System;

namespace FleetMind.Api.DTOs.Containers
{
    public class ContainerTrackingEventDto
    {
        public Guid Id { get; set; }
        public Guid ContainerId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
        public Guid RecordedByUserId { get; set; }
        public string RecordedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
