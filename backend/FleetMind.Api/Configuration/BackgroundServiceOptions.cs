namespace FleetMind.Api.Configuration;

public class BackgroundServiceOptions
{
    public int DelayedVoyageCheckIntervalMinutes { get; set; } = 15;
    public int MaintenanceOverdueCheckIntervalMinutes { get; set; } = 15;
    public int ExpiringCertificationCheckIntervalMinutes { get; set; } = 60;
}
