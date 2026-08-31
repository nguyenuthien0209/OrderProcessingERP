using EventBus.Contracts;
using MassTransit;
using Notifications.Worker.Services;

namespace Notifications.Worker.Consumers;

public class OrderCancelledConsumer : IConsumer<OrderCancelledIntegrationEvent>
{
    private readonly INotificationSender _sender;

    public OrderCancelledConsumer(INotificationSender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context) =>
        _sender.SendAsync(context.Message.OrderId, $"Your order was cancelled: {context.Message.Reason}", context.CancellationToken);
}
