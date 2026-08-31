using EventBus.Contracts;
using MassTransit;
using MediatR;
using Payments.Application.Payments.Commands.ChargePayment;

namespace Payments.Infrastructure.Messaging.Consumers;

public class InventoryReservedConsumer : IConsumer<InventoryReservedIntegrationEvent>
{
    private readonly ISender _sender;

    public InventoryReservedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<InventoryReservedIntegrationEvent> context) =>
        _sender.Send(new ChargePaymentCommand(context.Message.OrderId));
}
