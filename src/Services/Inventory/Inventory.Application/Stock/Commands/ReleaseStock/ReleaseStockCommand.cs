using Inventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Stock.Commands.ReleaseStock;

/// <summary>
/// Compensating action for a cancelled order. If nothing was ever reserved for this order
/// (e.g. it was cancelled because reservation itself failed), this is a no-op.
/// </summary>
public record ReleaseStockCommand(Guid OrderId) : IRequest;

public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand>
{
    private readonly IInventoryDbContext _dbContext;

    public ReleaseStockCommandHandler(IInventoryDbContext dbContext) => _dbContext = dbContext;

    public async Task Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        var reservations = await _dbContext.StockReservations
            .Where(r => r.OrderId == request.OrderId)
            .ToListAsync(cancellationToken);

        if (reservations.Count == 0)
            return;

        var productIds = reservations.Select(r => r.ProductId).ToList();
        var stockItems = await _dbContext.StockItems
            .Where(s => productIds.Contains(s.ProductId))
            .ToDictionaryAsync(s => s.ProductId, cancellationToken);

        foreach (var reservation in reservations)
        {
            if (stockItems.TryGetValue(reservation.ProductId, out var stockItem))
                stockItem.Release(reservation.Quantity);
        }

        _dbContext.StockReservations.RemoveRange(reservations);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
