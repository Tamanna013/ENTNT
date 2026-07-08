namespace FleetMind.Api.Configuration;

public class RateLimitingOptions
{
    public int AuthEndpointsPermitLimit { get; set; }
    public int AuthEndpointsWindowMinutes { get; set; }
    public int GeneralApiPermitLimit { get; set; }
    public int GeneralApiWindowMinutes { get; set; }
}
