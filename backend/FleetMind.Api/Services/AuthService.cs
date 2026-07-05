using AutoMapper;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Auth;
using FleetMind.Api.DTOs.Users;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Services;

/// <summary>
/// Implements user registration and login with BCrypt hashing and JWT tokens.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // 1. Check for duplicate email
        if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
        {
            throw new ConflictException($"A user with email '{dto.Email}' already exists.");
        }

        // 2. Ensure the default "User" role exists (created on-the-fly until proper seeding)
        var roleRepo = _unitOfWork.Repository<Role>();
        var existingRoles = await roleRepo.FindAsync(r => r.Name == "User");
        var userRole = existingRoles.FirstOrDefault();

        if (userRole is null)
        {
            userRole = new Role
            {
                Name = "User",
                Description = "Default role for registered users"
            };
            await roleRepo.AddAsync(userRole);
        }

        // 3. Create the user entity with hashed password
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

        // 4. Create the UserRole link via DbContext directly
        //    (UserRole doesn't inherit BaseEntity, so it can't use the generic repository)
        var userRoleLink = new UserRole
        {
            UserId = user.Id,
            RoleId = userRole.Id
        };
        await _unitOfWork.Context.UserRoles.AddAsync(userRoleLink);

        // 5. Persist everything atomically
        await _unitOfWork.SaveChangesAsync();

        // 6. Build response
        var roleNames = new List<string> { userRole.Name };
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);
        var expiresAt = _tokenService.GetAccessTokenExpiry();

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roleNames;

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // 1. Look up user by email — generic error if not found
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            // Deliberately identical message for both cases to prevent user enumeration
            throw new UnauthorizedAccessAppException("Invalid email or password.");
        }

        // 2. Load user's role names via DbContext (UserRole is not a BaseEntity)
        var roleNames = await _unitOfWork.Context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(
                _unitOfWork.Context.Roles.Where(r => !r.IsDeleted),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();

        // 3. Generate token
        var accessToken = _tokenService.GenerateAccessToken(user, roleNames);
        var expiresAt = _tokenService.GetAccessTokenExpiry();

        // 4. Map and return
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roleNames;

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }
}
