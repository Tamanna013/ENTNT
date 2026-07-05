namespace FleetMind.Api.Common.Exceptions;

/// <summary>
/// Application-level validation exception for business rule violations.
/// Named AppValidationException to avoid collision with FluentValidation.ValidationException
/// which will be introduced in Milestone 10.
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class AppValidationException : Exception
{
    /// <summary>
    /// Optional dictionary of field-level validation errors.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public AppValidationException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public AppValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
