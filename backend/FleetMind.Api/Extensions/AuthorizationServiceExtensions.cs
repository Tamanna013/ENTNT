using FleetMind.Api.Common.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace FleetMind.Api.Extensions;

public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// Configures role-based access control (RBAC) named policies.
    /// Future modules should add new named policies here rather than scattering raw Roles="..." strings across controllers,
    /// for maintainability — though [Authorize(Roles = AppRoles.X)] directly is also acceptable for single-role cases.
    /// </summary>
    public static IServiceCollection AddFleetMindAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(AppRoles.Admin));

            options.AddPolicy("AdminOrFleetManager", policy =>
                policy.RequireRole(AppRoles.Admin, AppRoles.FleetManager));

            options.AddPolicy("AdminOrCrewManager", policy =>
                policy.RequireRole(AppRoles.Admin, AppRoles.CrewManager));

            options.AddPolicy("AdminOrMaintenanceOfficer", policy =>
                policy.RequireRole(AppRoles.Admin, AppRoles.MaintenanceOfficer));
        });

        return services;
    }
}
