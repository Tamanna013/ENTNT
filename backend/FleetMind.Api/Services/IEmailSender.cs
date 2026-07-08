namespace FleetMind.Api.Services;

/// <summary>
/// Abstraction for sending emails. Mocked during development.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body);
}
