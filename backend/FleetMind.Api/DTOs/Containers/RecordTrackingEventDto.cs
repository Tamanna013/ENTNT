using System;

namespace FleetMind.Api.DTOs.Containers
{
    public class RecordTrackingEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }
}
