namespace FleetMind.Api.Common.Exceptions;

/// <summary>
/// Thrown when the current user lacks permission to perform the requested action.
/// Maps to HTTP 401 Unauthorized or 403 Forbidden.
/// </summary>
public class UnauthorizedAccessAppException : Exception
{
    public UnauthorizedAccessAppException(string message) : base(message)
    {
    }

    public UnauthorizedAccessAppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
