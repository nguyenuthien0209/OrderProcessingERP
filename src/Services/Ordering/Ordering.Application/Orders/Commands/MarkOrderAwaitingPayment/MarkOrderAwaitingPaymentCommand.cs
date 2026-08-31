using MediatR;
using Ordering.Application.Common.Interfaces;

namespace Ordering.Application.Orders.Commands.MarkOrderAwaitingPayment;

/// <summary>Internal status transition triggered by InventoryReserved; nothing further needs publishing here — Payments reacts to InventoryReserved directly.</summary>
public record MarkOrderAwaitingPaymentCommand(Guid OrderId) : IRequest;

public class MarkOrderAwaitingPaymentCommandHandler : IRequestHandler<MarkOrderAwaitingPaymentCommand>
{
    private readonly IOrderingDbContext _dbContext;

    public MarkOrderAwaitingPaymentCommandHandler(IOrderingDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(MarkOrderAwaitingPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FindAsync([request.OrderId], cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.MarkAwaitingPayment();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
