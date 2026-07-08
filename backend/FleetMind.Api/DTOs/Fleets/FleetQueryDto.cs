using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Fleets
{
    public class FleetQueryDto : PaginationQueryDto
    {
        public string? Status { get; set; }
    }
}
