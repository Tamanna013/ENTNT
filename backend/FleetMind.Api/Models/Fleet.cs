using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Fleet : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid HomePortId { get; set; }
        public Port? HomePort { get; set; }
        public string Status { get; set; } = FleetStatus.Active;

        public ICollection<Ship> Ships { get; set; } = new List<Ship>();
    }
}
