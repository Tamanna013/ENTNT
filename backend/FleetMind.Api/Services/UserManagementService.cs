using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Common.Constants;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Users;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FleetMind.Api.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailSender _emailSender;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            IEmailSender emailSender,
            ICurrentUserService currentUserService,
            ILogger<UserManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResultDto<UserDto>> GetUsersAsync(PaginationQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            query.PageNumber = Math.Max(1, query.PageNumber);

            var (items, totalCount) = await _unitOfWork.Users.GetPagedAsync(query);

            var dtos = items.Select(u =>
            {
                var dto = _mapper.Map<UserDto>(u);
                dto.Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList();
                return dto;
            }).ToList();

            return new PagedResultDto<UserDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException($"User with ID {id} not found.");
            }

            var roleNames = await _unitOfWork.Context.UserRoles
                .Where(ur => ur.UserId == id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = roleNames;

            return dto;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
            {
                throw new ConflictException($"User with email {dto.Email} already exists.");
            }

            var roles = new List<Role>();
            var roleRepo = _unitOfWork.Repository<Role>();
            if (dto.RoleNames != null && dto.RoleNames.Any())
            {
                foreach (var roleName in dto.RoleNames)
                {
                    var matchingRoles = await roleRepo.FindAsync(r => r.Name.ToLower() == roleName.ToLower());
                    var role = matchingRoles.FirstOrDefault();
                    if (role == null)
                    {
                        throw new AppValidationException($"Role '{roleName}' does not exist.");
                    }
                    roles.Add(role);
                }
            }
            else
            {
                // Default to User if none provided
                var matchingRoles = await roleRepo.FindAsync(r => r.Name == "User");
                var role = matchingRoles.FirstOrDefault();
                if (role != null) roles.Add(role);
            }

            var tempPassword = dto.Password;
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = _passwordHasher.Hash(tempPassword),
                IsActive = true,
                IsEmailVerified = true // Admin-created accounts are considered verified
            };

            await _unitOfWork.Users.AddAsync(user);

            foreach (var role in roles)
            {
                await _unitOfWork.Context.UserRoles.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }

            await _unitOfWork.SaveChangesAsync();

            // Note: In a production system this would prompt a forced password change on first login.
            _logger.LogInformation("Admin created user {email} with temporary password.", user.Email);
            await _emailSender.SendAsync(
                toEmail: user.Email,
                subject: "Welcome to FleetMind AI",
                body: $"Your account has been created by an administrator.\nEmail: {user.Email}\nTemporary Password: {tempPassword}\nPlease log in and change your password."
            );

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.Select(r => r.Name).ToList();

            return userDto;
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException($"User with ID {id} not found.");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            
            user.IsActive = dto.IsActive;

            // Role assignment logic during update
            if (dto.RoleNames != null)
            {
                var roleRepo = _unitOfWork.Repository<Role>();
                var newRoles = new List<Role>();
                
                foreach (var roleName in dto.RoleNames)
                {
                    var matchingRoles = await roleRepo.FindAsync(r => r.Name.ToLower() == roleName.ToLower());
                    var role = matchingRoles.FirstOrDefault();
                    if (role == null)
                    {
                        throw new AppValidationException($"Role '{roleName}' does not exist.");
                    }
                    newRoles.Add(role);
                }

                var existingLinks = await _unitOfWork.Context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
                _unitOfWork.Context.UserRoles.RemoveRange(existingLinks);

                foreach (var role in newRoles)
                {
                    await _unitOfWork.Context.UserRoles.AddAsync(new UserRole
                    {
                        UserId = id,
                        RoleId = role.Id
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var updatedRoleNames = await _unitOfWork.Context.UserRoles
                .Where(ur => ur.UserId == id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = updatedRoleNames;

            return userDto;
        }

        public async Task DeactivateUserAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException($"User with ID {id} not found.");
            }

            user.IsActive = false;
            _unitOfWork.Users.Remove(user); // Soft delete
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UserSettingsDto> GetMySettingsAsync()
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            if (userId == Guid.Empty) throw new UnauthorizedAccessException();

            var repo = _unitOfWork.Repository<UserSettings>();
            var settingsList = await repo.FindAsync(s => s.UserId == userId);
            var settings = settingsList.FirstOrDefault();

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    Theme = ThemePreference.System,
                    NotificationPreferencesJson = JsonSerializer.Serialize(GetDefaultNotificationPreferences())
                };

                try
                {
                    await repo.AddAsync(settings);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    settingsList = await repo.FindAsync(s => s.UserId == userId);
                    settings = settingsList.FirstOrDefault();
                    if (settings == null) throw;
                }
            }

            return MapToUserSettingsDto(settings);
        }

        public async Task<UserSettingsDto> UpdateMySettingsAsync(UpdateUserSettingsDto dto)
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            if (userId == Guid.Empty) throw new UnauthorizedAccessException();

            var repo = _unitOfWork.Repository<UserSettings>();
            var settingsList = await repo.FindAsync(s => s.UserId == userId);
            var settings = settingsList.FirstOrDefault();

            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                await repo.AddAsync(settings);
            }
            else
            {
                repo.Update(settings);
            }

            settings.Theme = dto.Theme;

            var validKeys = new HashSet<string>
            {
                NotificationType.MaintenanceOverdue,
                NotificationType.VoyageDelayed,
                NotificationType.CertificationExpiring,
                NotificationType.FuelAnomaly,
                NotificationType.General,
                NotificationType.IncidentReported
            };

            var filteredPrefs = dto.NotificationPreferences
                .Where(kvp => validKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            settings.NotificationPreferencesJson = JsonSerializer.Serialize(filteredPrefs);
            settings.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return MapToUserSettingsDto(settings);
        }

        public async Task<UserDto> UpdateOwnProfileAsync(UpdateOwnProfileDto dto)
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            if (userId == Guid.Empty) throw new UnauthorizedAccessException();

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("Current user not found.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AssignRolesAsync(Guid id, AssignRolesDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException($"User with ID {id} not found.");
            }

            var roleRepo = _unitOfWork.Repository<Role>();
            var newRoles = new List<Role>();
            
            foreach (var roleName in dto.RoleNames)
            {
                var matchingRoles = await roleRepo.FindAsync(r => r.Name.ToLower() == roleName.ToLower());
                var role = matchingRoles.FirstOrDefault();
                if (role == null)
                {
                    throw new AppValidationException($"Role '{roleName}' does not exist.");
                }
                newRoles.Add(role);
            }

            var existingLinks = await _unitOfWork.Context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            _unitOfWork.Context.UserRoles.RemoveRange(existingLinks);

            foreach (var role in newRoles)
            {
                await _unitOfWork.Context.UserRoles.AddAsync(new UserRole
                {
                    UserId = id,
                    RoleId = role.Id
                });
            }

            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = newRoles.Select(r => r.Name).ToList();

            return userDto;
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessAppException("Unauthenticated context.");
            }

            return await GetUserByIdAsync(userId.Value);
        }

        private static Dictionary<string, bool> GetDefaultNotificationPreferences()
        {
            return new Dictionary<string, bool>
            {
                { NotificationType.MaintenanceOverdue, true },
                { NotificationType.VoyageDelayed, true },
                { NotificationType.CertificationExpiring, true },
                { NotificationType.FuelAnomaly, true },
                { NotificationType.General, true },
                { NotificationType.IncidentReported, true }
            };
        }

        private static UserSettingsDto MapToUserSettingsDto(UserSettings settings)
        {
            return new UserSettingsDto
            {
                Theme = settings.Theme,
                NotificationPreferences = JsonSerializer.Deserialize<Dictionary<string, bool>>(settings.NotificationPreferencesJson) ?? new Dictionary<string, bool>(),
                UpdatedAt = settings.UpdatedAt ?? settings.CreatedAt
            };
        }
    }
}
