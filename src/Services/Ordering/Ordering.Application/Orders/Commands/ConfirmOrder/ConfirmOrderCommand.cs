using Common.Outbox;
using EventBus.Contracts;
using MediatR;
using Ordering.Application.Common.Interfaces;

namespace Ordering.Application.Orders.Commands.ConfirmOrder;

/// <summary>Sent when Payments has authorized funds for an order that already has inventory reserved.</summary>
public record ConfirmOrderCommand(Guid OrderId) : IRequest;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand>
{
    private readonly IOrderingDbContext _dbContext;

    public ConfirmOrderCommandHandler(IOrderingDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync([request.OrderId], cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.Confirm();

        _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(new OrderConfirmedIntegrationEvent
        {
            CorrelationId = order.Id,
            OrderId = order.Id
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
