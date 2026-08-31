using EventBus.Contracts;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.CancelOrder;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class InventoryReservationFailedConsumer : IConsumer<InventoryReservationFailedIntegrationEvent>
{
    private readonly ISender _sender;

    public InventoryReservationFailedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<InventoryReservationFailedIntegrationEvent> context) =>
        _sender.Send(new CancelOrderCommand(context.Message.OrderId, context.Message.Reason));
}
