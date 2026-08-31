using EventBus.Contracts;
using MassTransit;
using MediatR;
using Shipping.Application.Shipments.Commands.CreateShipment;

namespace Shipping.Infrastructure.Messaging.Consumers;

public class OrderConfirmedConsumer : IConsumer<OrderConfirmedIntegrationEvent>
{
    private readonly ISender _sender;

    public OrderConfirmedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<OrderConfirmedIntegrationEvent> context) =>
        _sender.Send(new CreateShipmentCommand(context.Message.OrderId));
}
