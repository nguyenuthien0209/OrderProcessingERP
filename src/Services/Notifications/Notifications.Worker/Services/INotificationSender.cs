namespace Notifications.Worker.Services;

public interface INotificationSender
{
    Task SendAsync(Guid orderId, string message, CancellationToken cancellationToken);
}
