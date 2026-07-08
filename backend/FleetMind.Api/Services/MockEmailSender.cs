namespace FleetMind.Api.Services;

/// <summary>
/// Mock email sender that simply logs the email to the console.
/// Swap this out for a real provider (e.g. SendGrid, SMTP) later.
/// </summary>
public class MockEmailSender : IEmailSender
{
    private readonly ILogger<MockEmailSender> _logger;

    public MockEmailSender(ILogger<MockEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation(
            "\n=== MOCK EMAIL ===\nTo: {to}\nSubject: {subject}\nBody: {body}\n===================", 
            toEmail, subject, body);
            
        return Task.CompletedTask;
    }
}
