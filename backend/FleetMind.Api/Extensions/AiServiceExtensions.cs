using FleetMind.Api.Configuration;
using FleetMind.Api.Services.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Extensions;

public static class AiServiceExtensions
{
    public static IServiceCollection AddFleetMindAiProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(AiProviderOptions.SectionName).Get<AiProviderOptions>() ?? new AiProviderOptions();
        services.AddSingleton(options);

        // Register concrete providers conditionally based on configuration
        if (options.Provider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<AzureOpenAiProvider>();
            services.AddSingleton<IAiProvider>(sp => new RateLimitedLoggingAiProviderDecorator(
                sp.GetRequiredService<AzureOpenAiProvider>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IOptions<AiRateLimitOptions>>()));
        }
        else if (options.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<GeminiAiProvider>();
            services.AddSingleton<IAiProvider>(sp => new RateLimitedLoggingAiProviderDecorator(
                sp.GetRequiredService<GeminiAiProvider>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IOptions<AiRateLimitOptions>>()));
        }
        else
        {
            services.AddSingleton<NullAiProvider>();
            services.AddSingleton<IAiProvider>(sp => new RateLimitedLoggingAiProviderDecorator(
                sp.GetRequiredService<NullAiProvider>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<IOptions<AiRateLimitOptions>>()));
        }

        return services;
    }
}
