using EventBus.Contracts;
using MassTransit;
using Notifications.Worker.Services;

namespace Notifications.Worker.Consumers;

public class OrderShippedConsumer : IConsumer<OrderShippedIntegrationEvent>
{
    private readonly INotificationSender _sender;

    public OrderShippedConsumer(INotificationSender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderShippedIntegrationEvent> context) =>
        _sender.SendAsync(context.Message.OrderId,
            $"Your order has shipped via {context.Message.Carrier}, tracking number {context.Message.TrackingNumber}.",
            context.CancellationToken);
}
