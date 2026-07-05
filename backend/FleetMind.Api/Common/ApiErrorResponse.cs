using System.Text.Json.Serialization;

namespace FleetMind.Api.Common;

/// <summary>
/// Standardized API error response returned by the global exception handling middleware.
/// Ensures all error responses share a consistent JSON shape.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// The unique trace identifier for this request, useful for correlating with server logs.
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code of the error response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// A user-facing error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information including stack trace.
    /// Only populated in Development environments — null in Production.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Details { get; set; }

    /// <summary>
    /// Optional field-level validation errors.
    /// Only populated for validation exceptions.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }
}
