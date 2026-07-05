namespace FleetMind.Api.Common.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with existing data (e.g., duplicate email).
/// Maps to HTTP 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
