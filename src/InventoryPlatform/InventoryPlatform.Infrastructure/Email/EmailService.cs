using InventoryPlatform.Application.Interfaces.Communication;
using Microsoft.Extensions.Logging;

namespace InventoryPlatform.Infrastructure.Email;

public sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            """
            Development email
            To: {Recipient}
            Subject: {Subject}
            Body:
            {Body}
            """,
            recipient,
            subject,
            body);

        return Task.CompletedTask;
    }
}