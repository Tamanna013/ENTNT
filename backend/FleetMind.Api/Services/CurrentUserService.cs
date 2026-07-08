using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FleetMind.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null && Guid.TryParse(idClaim.Value, out var userId))
                {
                    return userId;
                }
                return null;
            }
        }

        public string UserName
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var givenName = user.FindFirst(ClaimTypes.GivenName)?.Value;
                    var familyName = user.FindFirst(ClaimTypes.Surname)?.Value;
                    
                    if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(familyName))
                    {
                        return $"{givenName} {familyName}";
                    }
                    
                    return user.Identity.Name ?? "Unknown";
                }
                return "System";
            }
        }
    }
}
