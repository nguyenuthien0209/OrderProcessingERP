using EventBus.Contracts;
using MassTransit;
using MediatR;
using Payments.Application.Payments.Commands.CachePendingOrder;

namespace Payments.Infrastructure.Messaging.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly ISender _sender;

    public OrderCreatedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context) =>
        _sender.Send(new CachePendingOrderCommand(context.Message.OrderId, context.Message.CustomerId, context.Message.TotalAmount));
}
