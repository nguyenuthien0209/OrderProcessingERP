using EventBus.Contracts;
using MassTransit;
using Notifications.Worker.Services;

namespace Notifications.Worker.Consumers;

public class OrderConfirmedConsumer : IConsumer<OrderConfirmedIntegrationEvent>
{
    private readonly INotificationSender _sender;

    public OrderConfirmedConsumer(INotificationSender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderConfirmedIntegrationEvent> context) =>
        _sender.SendAsync(context.Message.OrderId, "Your order is confirmed and payment has been authorized.", context.CancellationToken);
}
