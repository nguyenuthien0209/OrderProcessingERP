using MediatR;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Common.Interfaces;
using Payments.Domain.Entities;

namespace Payments.Application.Payments.Commands.CachePendingOrder;

public record CachePendingOrderCommand(Guid OrderId, Guid CustomerId, decimal TotalAmount) : IRequest;

public class CachePendingOrderCommandHandler : IRequestHandler<CachePendingOrderCommand>
{
    private readonly IPaymentsDbContext _dbContext;

    public CachePendingOrderCommandHandler(IPaymentsDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(CachePendingOrderCommand request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.PendingOrders.AnyAsync(o => o.Id == request.OrderId, cancellationToken);
        if (exists)
            return;

        _dbContext.PendingOrders.Add(PendingOrder.Create(request.OrderId, request.CustomerId, request.TotalAmount));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
