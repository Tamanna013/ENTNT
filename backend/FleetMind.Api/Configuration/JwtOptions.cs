namespace FleetMind.Api.Configuration;

/// <summary>
/// Configuration options for JWT token generation and validation.
/// Non-secret values (Issuer, Audience, Expiry) live in appsettings.json.
/// The SigningKey lives ONLY in appsettings.Development.json (gitignored)
/// or environment variables in production.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
