using Common.Outbox;
using EventBus.Contracts;
using MediatR;
using Ordering.Application.Common.Interfaces;

namespace Ordering.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Sent either directly by a customer/operator, or by the saga itself when inventory reservation or
/// payment authorization fails. Publishing OrderCancelled is what triggers Inventory's compensating release.
/// </summary>
public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderingDbContext _dbContext;

    public CancelOrderCommandHandler(IOrderingDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync([request.OrderId], cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.Cancel(request.Reason);

        _dbContext.OutboxMessages.Add(OutboxMessageFactory.Create(new OrderCancelledIntegrationEvent
        {
            CorrelationId = order.Id,
            OrderId = order.Id,
            Reason = request.Reason
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
