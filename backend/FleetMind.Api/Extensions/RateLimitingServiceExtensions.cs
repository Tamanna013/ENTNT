using System.Threading.RateLimiting;
using FleetMind.Api.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FleetMind.Api.Extensions;

public static class RateLimitingServiceExtensions
{
    public static IServiceCollection AddFleetMindRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimiting"));

        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";
                
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
                }

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(new { message = "Too many requests. Please try again later." }),
                    token);
            };

            // Global Limiter - applying the general policy behavior logic
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var rateLimitOptions = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                
                // User id or IP fallback
                var partitionKey = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                    ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.GeneralApiPermitLimit,
                        Window = TimeSpan.FromMinutes(rateLimitOptions.GeneralApiWindowMinutes),
                        QueueLimit = 0
                    });
            });

            // Auth Policy - Strict IP based
            options.AddPolicy("auth", httpContext =>
            {
                var rateLimitOptions = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.AuthEndpointsPermitLimit,
                        Window = TimeSpan.FromMinutes(rateLimitOptions.AuthEndpointsWindowMinutes),
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
