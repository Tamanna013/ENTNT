using Asp.Versioning;
using FleetMind.Api.Configuration;
using FleetMind.Api.DTOs.Auth;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Controllers;

/// <summary>
/// Authentication endpoints for user registration, login, refresh, and logout.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "fleetmind_refresh_token";

    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IAuthService authService,
        IWebHostEnvironment env,
        IOptions<JwtOptions> jwtOptions)
    {
        _authService = authService;
        _env = env;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (response, refreshToken) = await _authService.RegisterAsync(dto);
        SetRefreshTokenCookie(refreshToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (response, refreshToken) = await _authService.LoginAsync(dto);
        SetRefreshTokenCookie(refreshToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token missing." });
        }

        var (response, newRefreshToken) = await _authService.RefreshAsync(refreshToken);
        SetRefreshTokenCookie(newRefreshToken);
        
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.LogoutAsync(refreshToken);
            Response.Cookies.Delete(RefreshTokenCookieName);
        }

        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto);
        // Always return 200 to prevent email enumeration
        return Ok(new { message = "If that email is registered, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(new { message = "Password has been successfully reset." });
    }

    [HttpGet("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        await _authService.VerifyEmailAsync(token);
        return Ok(new { message = "Email has been successfully verified." });
    }

    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        [FromServices] ICurrentUserService currentUserService)
    {
        var userId = currentUserService.UserId.Value;
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(new { message = "Password has been successfully changed." });
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            // During local development over http, Secure must be false.
            // In production, this MUST be true.
            Secure = !_env.IsDevelopment(), 
            // SameSite=Lax is the correct setting for this application's current cross-origin
            // frontend/backend deployment; revisit if the deployment topology changes to share a registrable domain.
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays)
        };

        Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
    }
}
