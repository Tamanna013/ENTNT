using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Users
{
    public class AssignRolesDto
    {
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}
