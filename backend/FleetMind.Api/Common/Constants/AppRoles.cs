namespace FleetMind.Api.Common.Constants;

/// <summary>
/// Canonical role list for the FleetMind platform.
/// Additional roles for specific modules (e.g., Voyage, Cargo, Incident) 
/// should be appended here as those modules are built.
/// Never redefine these roles ad hoc elsewhere.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string FleetManager = "FleetManager";
    public const string CrewManager = "CrewManager";
    public const string MaintenanceOfficer = "MaintenanceOfficer";
    public const string User = "User";
}
