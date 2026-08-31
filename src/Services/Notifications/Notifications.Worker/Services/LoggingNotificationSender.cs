using Microsoft.Extensions.Logging;

namespace Notifications.Worker.Services;

/// <summary>Stands in for a real email/SMS provider (SendGrid, Twilio, ...) — logs what would have been sent.</summary>
public class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) => _logger = logger;

    public Task SendAsync(Guid orderId, string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification for order {OrderId}: {Message}", orderId, message);
        return Task.CompletedTask;
    }
}
