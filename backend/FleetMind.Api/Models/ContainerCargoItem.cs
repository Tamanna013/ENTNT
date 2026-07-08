using System;

namespace FleetMind.Api.Models
{
    public class ContainerCargoItem
    {
        public Guid ContainerId { get; set; }
        public Container Container { get; set; } = null!;
        
        public Guid CargoId { get; set; }
        public Cargo Cargo { get; set; } = null!;
    }
}
