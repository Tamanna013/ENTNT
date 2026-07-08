using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories;

/// <summary>
/// Repository specific to refresh tokens.
/// </summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    /// <summary>
    /// Finds a refresh token by its exact SHA-256 hash.
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
}
