namespace FleetMind.Api.Configuration;

/// <summary>
/// Strongly-typed options for database connectivity.
/// Bound from the "ConnectionStrings" configuration section.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Configuration section name used by the options pattern.
    /// </summary>
    public const string SectionName = "ConnectionStrings";

    /// <summary>
    /// The default SQL Server connection string.
    /// </summary>
    public string DefaultConnection { get; set; } = string.Empty;
}
