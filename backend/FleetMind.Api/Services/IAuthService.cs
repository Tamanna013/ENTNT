using FleetMind.Api.DTOs.Auth;

namespace FleetMind.Api.Services;

/// <summary>
/// Authentication service interface for registration and login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the default "User" role and returns an auth response with JWT and a raw refresh token.
    /// Throws ConflictException if the email is already registered.
    /// </summary>
    Task<(AuthResponseDto response, string refreshToken)> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Authenticates a user by email and password, returning an auth response with JWT and a raw refresh token.
    /// Throws UnauthorizedAccessAppException with a generic message on failure.
    /// </summary>
    Task<(AuthResponseDto response, string refreshToken)> LoginAsync(LoginDto dto);

    /// <summary>
    /// Refreshes an access token using a valid, unexpired refresh token.
    /// </summary>
    Task<(AuthResponseDto response, string newRefreshToken)> RefreshAsync(string presentedRefreshToken);

    /// <summary>
    /// Revokes a refresh token, logging the user out of that specific session.
    /// </summary>
    Task LogoutAsync(string presentedRefreshToken);

    /// <summary>
    /// Initiates the password reset flow. Does not throw if email is missing to prevent enumeration.
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordDto dto);

    /// <summary>
    /// Resets a password using a valid, unused password reset token.
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordDto dto);

    /// <summary>
    /// Marks an account's email as verified using a valid, unused verification token.
    /// </summary>
    Task VerifyEmailAsync(string rawToken);

    /// <summary>
    /// Changes the password of an authenticated user using their current password.
    /// Also revokes all their existing refresh tokens as a security measure.
    /// </summary>
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}
