using System;
using System.Collections.Generic;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Container : BaseEntity
    {
        public string ContainerNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = ContainerStatus.Empty;
        
        public Guid? CurrentVoyageId { get; set; }
        public Voyage? CurrentVoyage { get; set; }
        
        public ICollection<ContainerCargoItem> ContainerCargoItems { get; set; } = new List<ContainerCargoItem>();
        public ICollection<ContainerTrackingEvent> TrackingEvents { get; set; } = new List<ContainerTrackingEvent>();
    }
}
