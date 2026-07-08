using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Ports
{
    public class PortQueryDto : PaginationQueryDto
    {
        public string? Country { get; set; }
    }
}
