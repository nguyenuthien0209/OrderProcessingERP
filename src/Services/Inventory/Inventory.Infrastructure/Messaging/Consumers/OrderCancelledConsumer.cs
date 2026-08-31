using EventBus.Contracts;
using Inventory.Application.Stock.Commands.ReleaseStock;
using MassTransit;
using MediatR;

namespace Inventory.Infrastructure.Messaging.Consumers;

/// <summary>The compensating side of the saga: whatever caused the cancellation, release any stock held for this order.</summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelledIntegrationEvent>
{
    private readonly ISender _sender;

    public OrderCancelledConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context) =>
        _sender.Send(new ReleaseStockCommand(context.Message.OrderId));
}
