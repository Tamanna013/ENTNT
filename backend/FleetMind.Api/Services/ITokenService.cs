using FleetMind.Api.Models;

namespace FleetMind.Api.Services;

/// <summary>
/// Generates signed JWT access tokens with user identity and role claims.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a signed JWT containing sub, email, name, and role claims.
    /// </summary>
    /// <returns>The serialized JWT string.</returns>
    string GenerateAccessToken(User user, IEnumerable<string> roleNames);

    /// <summary>
    /// Returns the absolute UTC expiry time for a newly generated token.
    /// </summary>
    DateTime GetAccessTokenExpiry();
}
