using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories;

/// <summary>
/// User-specific repository interface extending the generic repository
/// with custom query methods needed for authentication and user management.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Gets a non-deleted user by their email address (case-insensitive).
    /// Returns null if no matching user is found.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks whether a non-deleted user with the given email already exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);
}
