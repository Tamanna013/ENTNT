using FleetMind.Api.DTOs.Auth;

namespace FleetMind.Api.Services;

/// <summary>
/// Authentication service interface for registration and login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the default "User" role and returns an auth response with JWT.
    /// Throws ConflictException if the email is already registered.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Authenticates a user by email and password, returning an auth response with JWT.
    /// Throws UnauthorizedAccessAppException with a generic message on failure
    /// (intentionally does not reveal whether the email or password was wrong).
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
