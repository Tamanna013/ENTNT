namespace FleetMind.Api.Services;

/// <summary>
/// Abstraction over password hashing to allow easy testing and swapping implementations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a BCrypt hash of the given plaintext password.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext password against a stored BCrypt hash.
    /// </summary>
    bool Verify(string password, string hash);
}
