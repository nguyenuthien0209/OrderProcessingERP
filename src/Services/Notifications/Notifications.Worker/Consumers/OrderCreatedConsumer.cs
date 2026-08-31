using EventBus.Contracts;
using MassTransit;
using Notifications.Worker.Services;

namespace Notifications.Worker.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly INotificationSender _sender;

    public OrderCreatedConsumer(INotificationSender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context) =>
        _sender.SendAsync(context.Message.OrderId,
            $"Thanks for your order! We're processing {context.Message.Items.Count} item(s) totalling {context.Message.TotalAmount:C}.",
            context.CancellationToken);
}
