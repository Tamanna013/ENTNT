using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FleetMind.Api.Models;

namespace FleetMind.Api.Services;

public class NotificationRecipientResolver : INotificationRecipientResolver
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationRecipientResolver(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Guid>> GetUserIdsByRolesAsync(string notificationType, params string[] roleNames)
    {
        if (roleNames == null || roleNames.Length == 0)
        {
            return new List<Guid>();
        }

        var candidateUsers = await _unitOfWork.Context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted && u.UserRoles.Any(ur => roleNames.Contains(ur.Role.Name)))
            .Select(u => new { 
                u.Id, 
                Settings = _unitOfWork.Context.Set<UserSettings>().FirstOrDefault(s => s.UserId == u.Id) 
            })
            .ToListAsync();

        var result = new List<Guid>();

        foreach (var user in candidateUsers)
        {
            bool isEnabled = true;

            if (user.Settings != null && !string.IsNullOrEmpty(user.Settings.NotificationPreferencesJson))
            {
                try
                {
                    var prefs = JsonSerializer.Deserialize<Dictionary<string, bool>>(user.Settings.NotificationPreferencesJson);
                    if (prefs != null && prefs.ContainsKey(notificationType))
                    {
                        isEnabled = prefs[notificationType];
                    }
                }
                catch
                {
                    // Ignore parsing errors, default to enabled
                }
            }

            if (isEnabled)
            {
                result.Add(user.Id);
            }
        }

        return result.Distinct().ToList();
    }
}
