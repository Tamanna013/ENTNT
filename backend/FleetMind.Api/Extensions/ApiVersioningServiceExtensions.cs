using Asp.Versioning;

namespace FleetMind.Api.Extensions;

/// <summary>
/// Extension methods for configuring API versioning.
/// </summary>
public static class ApiVersioningServiceExtensions
{
    /// <summary>
    /// Adds formal API versioning with URL segment strategy (api/v1/, api/v2/, etc.).
    /// Default version is 1.0 and is assumed when the client doesn't specify one.
    /// </summary>
    public static IServiceCollection AddFleetMindApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                // Format version as 'v'major[.minor][-status], e.g. v1, v2.1
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}
