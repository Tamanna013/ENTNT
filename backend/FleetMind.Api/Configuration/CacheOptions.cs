namespace FleetMind.Api.Configuration
{
    public class CacheOptions
    {
        public int DefaultAbsoluteExpirationMinutes { get; set; } = 5;
        public int AnalyticsExpirationMinutes { get; set; } = 15;
    }
}
