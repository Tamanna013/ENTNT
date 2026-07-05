namespace FleetMind.Api.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity or resource cannot be found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.")
    {
    }
}
