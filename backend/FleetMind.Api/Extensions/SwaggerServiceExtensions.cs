using Microsoft.OpenApi.Models;

namespace FleetMind.Api.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI with JWT bearer auth support.
/// </summary>
public static class SwaggerServiceExtensions
{
    /// <summary>
    /// Adds Swagger with a v1 API document, JWT Bearer security definition,
    /// and a global security requirement so every endpoint shows the padlock icon.
    /// </summary>
    public static IServiceCollection AddFleetMindSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FleetMind AI API",
                Version = "v1",
                Description = "AI-Powered Maritime Operations Management Platform API",
                Contact = new OpenApiContact
                {
                    Name = "FleetMind Team"
                }
            });

            // JWT Bearer security definition
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter your JWT token: **Bearer &lt;token&gt;**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", bearerScheme);

            // Global security requirement — every endpoint shows a padlock
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { bearerScheme, Array.Empty<string>() }
            });
        });

        return services;
    }
}
