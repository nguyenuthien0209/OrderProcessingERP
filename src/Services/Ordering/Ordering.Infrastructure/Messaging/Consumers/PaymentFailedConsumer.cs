using EventBus.Contracts;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.CancelOrder;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedIntegrationEvent>
{
    private readonly ISender _sender;

    public PaymentFailedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context) =>
        _sender.Send(new CancelOrderCommand(context.Message.OrderId, context.Message.Reason));
}
