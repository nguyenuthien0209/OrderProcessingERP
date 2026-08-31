using EventBus.Contracts;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.MarkOrderShipped;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class OrderShippedConsumer : IConsumer<OrderShippedIntegrationEvent>
{
    private readonly ISender _sender;

    public OrderShippedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderShippedIntegrationEvent> context) =>
        _sender.Send(new MarkOrderShippedCommand(context.Message.OrderId, context.Message.Carrier, context.Message.TrackingNumber));
}
