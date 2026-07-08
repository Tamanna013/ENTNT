using AutoMapper;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Configuration;
using FleetMind.Api.DTOs.Auth;
using FleetMind.Api.DTOs.Users;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Services;

/// <summary>
/// Implements user registration, login, refresh token rotation, password reset, and email verification.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly JwtOptions _jwtOptions;
    private readonly AccountLockoutOptions _lockoutOptions;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper,
        IOptions<JwtOptions> jwtOptions,
        IOptions<AccountLockoutOptions> lockoutOptions,
        IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
        _jwtOptions = jwtOptions.Value;
        _lockoutOptions = lockoutOptions.Value;
        _emailSender = emailSender;
    }

    public async Task<(AuthResponseDto response, string refreshToken)> RegisterAsync(RegisterDto dto)
    {
        // 1. Check for duplicate email
        if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
        {
            throw new ConflictException($"A user with email '{dto.Email}' already exists.");
        }

        // 2. Ensure the default "User" role exists
        var roleRepo = _unitOfWork.Repository<Role>();
        var existingRoles = await roleRepo.FindAsync(r => r.Name == "User");
        var userRole = existingRoles.FirstOrDefault();

        if (userRole is null)
        {
            throw new InvalidOperationException("Required role 'User' is missing from the database - seeding may not have run correctly.");
        }

        // 3. Create the user entity with hashed password (IsEmailVerified defaults to false)
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);

        // 4. Create the UserRole link
        var userRoleLink = new UserRole
        {
            UserId = user.Id,
            RoleId = userRole.Id
        };
        await _unitOfWork.Context.UserRoles.AddAsync(userRoleLink);

        // 5. Generate Refresh Token
        var refreshTokenRaw = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(refreshTokenRaw);
        
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays)
        };
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);

        // 6. Generate Email Verification Token
        var verifyTokenRaw = _tokenService.GenerateRefreshToken();
        var verifyTokenHash = _tokenService.HashToken(verifyTokenRaw);

        var verifyToken = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = verifyTokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 hours expiry
        };
        await _unitOfWork.Repository<EmailVerificationToken>().AddAsync(verifyToken);

        // 7. Persist everything atomically
        await _unitOfWork.SaveChangesAsync();

        // 8. Send mock verification email
        var verifyLink = $"https://localhost:5173/verify-email?token={verifyTokenRaw}";
        await _emailSender.SendAsync(
            toEmail: user.Email,
            subject: "Verify your email address",
            body: $"Welcome to FleetMind! Please verify your email by clicking the link below:\n\n{verifyLink}");

        // 9. Build response
        var roleNames = new List<string> { userRole.Name };
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);
        var expiresAt = _tokenService.GetAccessTokenExpiry();

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roleNames;

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            User = userDto
        };

        return (response, refreshTokenRaw);
    }

    public async Task<(AuthResponseDto response, string refreshToken)> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

        if (user is null)
        {
            // CRITICAL: Maintain exact same behavior for nonexistent accounts to prevent enumeration
            throw new UnauthorizedAccessAppException("Invalid email or password.");
        }

        // Account is found, check if it's currently locked out
        if (user.LockedOutUntil.HasValue && user.LockedOutUntil.Value > DateTime.UtcNow)
        {
            // Note: This reveals that the account exists, which is a standard tradeoff
            // of having an account lockout feature.
            throw new AppValidationException(
                $"This account is temporarily locked due to repeated failed login attempts. Please try again after {user.LockedOutUntil.Value:HH:mm:ss} UTC.");
        }

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash) || !user.IsActive)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= _lockoutOptions.MaxFailedAttempts)
            {
                user.LockedOutUntil = DateTime.UtcNow.AddMinutes(_lockoutOptions.LockoutDurationMinutes);
            }
            await _unitOfWork.SaveChangesAsync();
            throw new UnauthorizedAccessAppException("Invalid email or password.");
        }

        // Successful login: reset lockout state
        user.FailedLoginAttempts = 0;
        user.LockedOutUntil = null;

        var roleNames = await _unitOfWork.Context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(
                _unitOfWork.Context.Roles.Where(r => !r.IsDeleted),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();

        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);
        var expiresAt = _tokenService.GetAccessTokenExpiry();

        var refreshTokenRaw = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(refreshTokenRaw);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays)
        };
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roleNames;

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            User = userDto
        };

        return (response, refreshTokenRaw);
    }

    public async Task<(AuthResponseDto response, string newRefreshToken)> RefreshAsync(string presentedRefreshToken)
    {
        var tokenHash = _tokenService.HashToken(presentedRefreshToken);
        var existingToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash);

        if (existingToken == null || existingToken.IsExpired || existingToken.IsRevoked)
        {
            throw new UnauthorizedAccessAppException("Invalid or expired refresh token.");
        }

        existingToken.RevokedAt = DateTime.UtcNow;

        var user = await _unitOfWork.Users.GetByIdAsync(existingToken.UserId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessAppException("User no longer active.");
        }

        var roleNames = await _unitOfWork.Context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(
                _unitOfWork.Context.Roles.Where(r => !r.IsDeleted),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();

        var newAccessToken = _tokenService.GenerateAccessToken(user, roleNames);
        var newAccessTokenExpiry = _tokenService.GetAccessTokenExpiry();

        var newRefreshTokenRaw = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashToken(newRefreshTokenRaw);

        existingToken.ReplacedByTokenHash = newRefreshTokenHash;

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays)
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roleNames;

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            ExpiresAt = newAccessTokenExpiry,
            User = userDto
        };

        return (response, newRefreshTokenRaw);
    }

    public async Task LogoutAsync(string presentedRefreshToken)
    {
        var tokenHash = _tokenService.HashToken(presentedRefreshToken);
        var existingToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash);

        if (existingToken != null && !existingToken.IsRevoked)
        {
            existingToken.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        
        // If not found, do nothing but still return normally (prevents enumeration).
        if (user == null)
            return;

        var resetTokenRaw = _tokenService.GenerateRefreshToken(); // Reusing the secure string generator
        var resetTokenHash = _tokenService.HashToken(resetTokenRaw);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = resetTokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1) // 1 hour expiry
        };

        await _unitOfWork.Repository<PasswordResetToken>().AddAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();

        var resetLink = $"https://localhost:5173/reset-password?token={resetTokenRaw}";
        await _emailSender.SendAsync(
            toEmail: user.Email,
            subject: "Password Reset Request",
            body: $"You have requested a password reset. Please click the link below to set a new password:\n\n{resetLink}");
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var tokenHash = _tokenService.HashToken(dto.Token);
        
        var resetTokenRepo = _unitOfWork.Repository<PasswordResetToken>();
        var tokens = await resetTokenRepo.FindAsync(rt => 
            rt.TokenHash == tokenHash && 
            rt.UsedAt == null && 
            rt.ExpiresAt > DateTime.UtcNow &&
            !rt.IsDeleted);
            
        var resetToken = tokens.FirstOrDefault();

        if (resetToken == null)
        {
            throw new AppValidationException("Invalid or expired reset token.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(resetToken.UserId);
        if (user == null)
        {
            throw new AppValidationException("Invalid or expired reset token.");
        }

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        resetToken.UsedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task VerifyEmailAsync(string rawToken)
    {
        var tokenHash = _tokenService.HashToken(rawToken);

        var verifyTokenRepo = _unitOfWork.Repository<EmailVerificationToken>();
        var tokens = await verifyTokenRepo.FindAsync(rt => 
            rt.TokenHash == tokenHash && 
            rt.VerifiedAt == null && 
            rt.ExpiresAt > DateTime.UtcNow &&
            !rt.IsDeleted);
            
        var verifyToken = tokens.FirstOrDefault();

        if (verifyToken == null)
        {
            throw new AppValidationException("Invalid or expired verification token.");
        }



        var user = await _unitOfWork.Users.GetByIdAsync(verifyToken.UserId);
        if (user == null)
        {
            throw new AppValidationException("Invalid or expired verification token.");
        }

        user.IsEmailVerified = true;
        verifyToken.VerifiedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessAppException("User not found.");
        }

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessAppException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);

        // Revoke all existing refresh tokens for this user
        var activeTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow && !rt.IsDeleted);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }
        
        // Note: In this implementation, ALL sessions for this user (including the current one) 
        // are revoked. This is a deliberate security choice to ensure maximum security upon 
        // password change. The UX implication is that the user making the change will need 
        // to log back in themselves with their new password, ensuring complete session rotation.

        await _unitOfWork.SaveChangesAsync();
    }
}
