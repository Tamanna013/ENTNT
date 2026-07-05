using System.Net;
using System.Text.Json;
using FleetMind.Api.Common;
using FleetMind.Api.Common.Exceptions;

namespace FleetMind.Api.Middleware;

/// <summary>
/// Global exception handling middleware. Catches all unhandled exceptions,
/// maps them to appropriate HTTP status codes, and returns a consistent
/// JSON error response. Must be registered first in the pipeline so it
/// wraps all subsequent middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var (statusCode, message) = MapException(exception);

        _logger.LogError(
            exception,
            "Unhandled exception [{ExceptionType}] — TraceId: {TraceId}, StatusCode: {StatusCode}, Message: {Message}",
            exception.GetType().Name,
            traceId,
            statusCode,
            exception.Message);

        var response = new ApiErrorResponse
        {
            TraceId = traceId,
            StatusCode = statusCode,
            Message = message
        };

        // Only include stack trace details in Development
        if (_env.IsDevelopment())
        {
            response.Details = $"{exception.Message}\n{exception.StackTrace}";
        }

        // Attach field-level errors for validation exceptions
        if (exception is AppValidationException validationEx && validationEx.Errors.Count > 0)
        {
            response.Errors = validationEx.Errors;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    private static (int StatusCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            NotFoundException ex =>
                ((int)HttpStatusCode.NotFound, ex.Message),

            AppValidationException ex =>
                ((int)HttpStatusCode.BadRequest, ex.Message),

            UnauthorizedAccessAppException ex =>
                ((int)HttpStatusCode.Unauthorized, ex.Message),

            ConflictException ex =>
                ((int)HttpStatusCode.Conflict, ex.Message),

            _ =>
                ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
    }
}
