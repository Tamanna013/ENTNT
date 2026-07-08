using FleetMind.Api.Configuration;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityHeadersOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            // Prevents a browser from MIME-sniffing a JSON response as something executable
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // Prevents this API's responses from ever being framed
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // A sensible, privacy-conscious default limiting referrer information leakage
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // A deliberately strict, default-deny policy appropriate SPECIFICALLY because this is a pure JSON API 
            // rendering no HTML itself. A server-rendered HTML application would need a more nuanced, permissive 
            // policy allowing its own scripts/styles, but this API has no such content to allow.
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'");

            // Deny these browser features by default, a defensive default even though this API itself has no direct browser-feature usage
            context.Response.Headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=()");

            // Only added when SecurityHeadersOptions.EnableHsts is true.
            if (_options.EnableHsts)
            {
                var maxAge = _options.HstsMaxAgeDays * 86400;
                context.Response.Headers.Append("Strict-Transport-Security", $"max-age={maxAge}; includeSubDomains");
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
