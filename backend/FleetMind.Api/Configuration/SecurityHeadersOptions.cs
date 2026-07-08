namespace FleetMind.Api.Configuration;

public class SecurityHeadersOptions
{
    public bool EnableHsts { get; set; }
    public int HstsMaxAgeDays { get; set; }
}
