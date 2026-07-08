using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetMind.Api.Services;

public interface INotificationRecipientResolver
{
    Task<List<Guid>> GetUserIdsByRolesAsync(string notificationType, params string[] roleNames);
}
