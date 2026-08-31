using Inventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Stock.Queries.GetStockByProductId;

public record StockDto(Guid ProductId, int QuantityOnHand, int QuantityReserved, int QuantityAvailable);

public record GetStockByProductIdQuery(Guid ProductId) : IRequest<StockDto?>;

public class GetStockByProductIdQueryHandler : IRequestHandler<GetStockByProductIdQuery, StockDto?>
{
    private readonly IInventoryDbContext _dbContext;

    public GetStockByProductIdQueryHandler(IInventoryDbContext dbContext) => _dbContext = dbContext;

    public async Task<StockDto?> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
    {
        var stockItem = await _dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        return stockItem is null
            ? null
            : new StockDto(stockItem.ProductId, stockItem.QuantityOnHand, stockItem.QuantityReserved, stockItem.QuantityAvailable);
    }
}
