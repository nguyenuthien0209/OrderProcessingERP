using EventBus.Contracts;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.MarkOrderAwaitingPayment;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class InventoryReservedConsumer : IConsumer<InventoryReservedIntegrationEvent>
{
    private readonly ISender _sender;

    public InventoryReservedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<InventoryReservedIntegrationEvent> context) =>
        _sender.Send(new MarkOrderAwaitingPaymentCommand(context.Message.OrderId));
}
