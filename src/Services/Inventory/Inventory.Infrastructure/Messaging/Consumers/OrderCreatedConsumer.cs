using EventBus.Contracts;
using Inventory.Application.Stock.Commands.ReserveStock;
using MassTransit;
using MediatR;

namespace Inventory.Infrastructure.Messaging.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private readonly ISender _sender;

    public OrderCreatedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context) =>
        _sender.Send(new ReserveStockCommand(
            context.Message.OrderId,
            context.Message.Items.Select(i => new ReserveStockItemDto(i.ProductId, i.Quantity)).ToList()));
}
