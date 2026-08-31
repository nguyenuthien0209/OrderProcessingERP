using EventBus.Contracts;
using MassTransit;
using MediatR;
using Ordering.Application.Orders.Commands.ConfirmOrder;

namespace Ordering.Infrastructure.Messaging.Consumers;

public class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorizedIntegrationEvent>
{
    private readonly ISender _sender;

    public PaymentAuthorizedConsumer(ISender sender) => _sender = sender;

    public Task Consume(ConsumeContext<PaymentAuthorizedIntegrationEvent> context) =>
        _sender.Send(new ConfirmOrderCommand(context.Message.OrderId));
}
