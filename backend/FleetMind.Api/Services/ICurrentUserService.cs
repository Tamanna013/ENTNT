using System;

namespace FleetMind.Api.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string UserName { get; }
    }
}
