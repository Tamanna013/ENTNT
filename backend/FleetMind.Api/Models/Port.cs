using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Port : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string UnLocode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
